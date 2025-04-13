using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using System;
using UnityEngine.UIElements;
using System.Collections;
using System.Linq;
using RangeAttribute = UnityEngine.RangeAttribute;
public class DungeonGenerator : MonoBehaviour
{
    #region variables
    [Header("Generation Method")]
    [SerializeField] private float timeBetweenSteps = 0.1f;
    public enum PathSearchingMethods { DepthFirst, BreadthFirst }
    [SerializeField] private bool removeCyclingPaths = true;
    [ShowIf("removeCyclingPaths")][SerializeField] private PathSearchingMethods pathSearchingMethod = PathSearchingMethods.DepthFirst;

    [Header("Seed")]
    [SerializeField] private bool useSeed = false;
    [ShowIf("useSeed")][SerializeField] private int seed = 1;

    [Header("Dungeon Stats")]
    [SerializeField] private int dungeonWidth = 1000;
    [SerializeField] private int dungeonHeight = 1000;

    [Header("Room Stats")]
    [SerializeField] private int roomMinWidth = 100;
    [SerializeField] private int roomMinHeight = 100;
    [MinValue(1)][SerializeField] private int wallBuffer = 1;

    [Header("Door Stats")]
    [SerializeField] private int doorSize = 6;

    [Header("Generation Stats")]
    [SerializeField] private bool removeRooms = true;
    [ShowIf("removeRooms")][Range(0f, 100f)][SerializeField] private float percentageOfRoomsToRemove = 10f;
    public enum Sizes { Smallest, Biggest, Random }
    [ShowIf("removeRooms")][SerializeField] private Sizes roomSizeToBeRemoved = Sizes.Smallest;
    [SerializeField] private bool makeNonSquareRooms = false;
    [ShowIf("makeNonSquareRooms")][RangeAttribute(0f, 100f)][SerializeField] private float chanceToMakeInterestingRoom = 20f;

    [Header("Visualization")]
    [SerializeField] private bool showRooms = true;
    [SerializeField] private bool showDoors = true;
    [SerializeField] private bool showDungeonOutLine = true;
    [SerializeField] private bool showRemovedRooms = true;
    [SerializeField] private bool showGraph = true;

    // Not in inspector
    public class Room
    {
        public RectInt area;
        public int size;
        public bool isDoor = false;
    }
    public Graph<Room> rooms;
    public Graph<Room> doors;
    private List<Room> removedRooms;

    private RectInt dungeon;
    private Room firstRoom;

    // Used for coroutines
    private bool finishedSplitting = false;
    private bool finishedFindingConnections = false;
    private bool finishedRemoval = false;
    private bool finishedPathing = false;
    private bool finishedDoors = false;

    // Random
    private System.Random random;

    // Sorting
    public enum SortingOrders { SmallestToBiggest, BiggestToSmallest }
    #endregion
    private void Start()
    {
        dungeon = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        firstRoom = new Room();
        firstRoom.area = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        rooms = new Graph<Room>();
        doors = new Graph<Room>();
        removedRooms = new List<Room>();
    }

    #region RoomSplitting
    // Coroutine for visualization
    private IEnumerator SplitRooms()
    {
        bool changedARoom = true;
        // go through the list of rooms splitAmount times
        while (changedARoom)
        {
            List<Room> unfinishedRooms = rooms.KeysToList();
            changedARoom = false;

            for (int i = unfinishedRooms.Count - 1; i >= 0; i--)
            {
                // Choose how to split
                if (random.Next(0, 2) == 1)
                {
                    var variable = rooms.adjacencyList;
                    // Split horizontally if the room will not become too small, otherwise try vertically
                    if (unfinishedRooms[i].area.height / 2 > roomMinHeight)
                    {
                        SplitHorizontally(unfinishedRooms[i]);
                        changedARoom = true;
                        yield return new WaitForSeconds(timeBetweenSteps);
                    }

                    else if (unfinishedRooms[i].area.width / 2 > roomMinWidth)
                    {
                        SplitVertically(unfinishedRooms[i]);
                        changedARoom = true;
                        yield return new WaitForSeconds(timeBetweenSteps);
                    }
                }
                else
                {
                    // Split vertically if the room will not become too small, otherwise try horizontally
                    if (unfinishedRooms[i].area.width / 2 > roomMinWidth)
                    {
                        SplitVertically(unfinishedRooms[i]);
                        changedARoom = true;
                        yield return new WaitForSeconds(timeBetweenSteps);
                    }

                    else if (unfinishedRooms[i].area.height / 2 > roomMinHeight)
                    {
                        SplitHorizontally(unfinishedRooms[i]);
                        changedARoom = true;
                        yield return new WaitForSeconds(timeBetweenSteps);
                    }
                }
            }
        }

        finishedSplitting = true;
    }
    /// <summary>
    /// Split room horizontally (reduce the y/height)
    /// </summary>
    /// <param name="roomObject"></param>
    private void SplitHorizontally(Room roomObject)
    {
        // some casting to make the script readable
        RectInt room = roomObject.area;

        // create 2 room sizes, randomly picked based on the size of the room being split
        int randomSize = random.Next(roomMinHeight, room.height - roomMinHeight + 1);
        int restSize = room.height - randomSize;

        // Create two new RectInts for the split rooms
        RectInt roomTop = new RectInt(room.x, room.y + randomSize - wallBuffer, room.width, restSize + wallBuffer);
        RectInt roomBottom = new RectInt(room.x, room.y, room.width, randomSize);

        // Create a new Room object for the top room
        Room roomObjectTop = new Room();
        roomObjectTop.area = roomTop;
        roomObjectTop.size = roomTop.width * roomTop.height;

        // Create a new Room object for the bottom room
        Room roomObjectBottom = new Room();
        roomObjectBottom.area = roomBottom;
        roomObjectBottom.size = roomBottom.width * roomBottom.height;

        // Add the newly created rooms to the room list
        rooms.AddNode(roomObjectTop);
        rooms.AddNode(roomObjectBottom);

        // Remove the original room from the room list
        rooms.RemoveNode(roomObject);
    }
    /// <summary>
    /// Split room vertically (reduce the x/width)
    /// </summary>
    /// <param name="roomObject"></param>
    private void SplitVertically(Room roomObject)
    {
        // some casting to make the script readable
        RectInt room = roomObject.area;

        // create 2 room sizes, randomly picked based on the size of the room being split
        int randomSize = random.Next(roomMinWidth, room.width - roomMinWidth + 1);
        int restSize = room.width - randomSize;

        // Create two new RectInts for the split rooms
        RectInt roomLeft = new RectInt(room.x, room.y, randomSize, room.height);
        RectInt roomRight = new RectInt(room.x + randomSize - wallBuffer, room.y, restSize + wallBuffer, room.height);

        // Create a new Room object for the left room
        Room roomObjectLeft = new Room();
        roomObjectLeft.area = roomLeft;
        roomObjectLeft.size = roomLeft.width * roomLeft.height;

        // Create a new Room object for the right room
        Room roomObjectRight = new Room();
        roomObjectRight.area = roomRight;
        roomObjectRight.size = roomRight.width * roomRight.height;

        // Add the newly created rooms to the room list
        rooms.AddNode(roomObjectLeft);
        rooms.AddNode(roomObjectRight);

        // Remove the original room from the room list
        rooms.RemoveNode(roomObject);
    }
    #endregion

    #region ConnectionFinding
    /// <summary>
    /// Makes all rooms that overlap and could place a door be eachothers neighbors
    /// </summary>
    /// <returns></returns>
    private IEnumerator FindConnections()
    {
        List<Room> list = rooms.KeysToList();

        // Complexity: (O(n^2))
        for (int i = 0; i < list.Count; i++)
        {
            for (int k = i + 1; k < list.Count; k++)
            {
                if (AlgorithmsUtils.Intersects(list[i].area, list[k].area))
                {
                    RectInt intersect = AlgorithmsUtils.Intersect(list[i].area, list[k].area);
                    if (intersect.width >= wallBuffer * 2 + doorSize || intersect.height >= wallBuffer * 2 + doorSize)
                    {
                        rooms.AddNeighbor(list[i], list[k]);
                    }
                }
            }
        }
        yield return new WaitForSeconds(timeBetweenSteps);
        finishedFindingConnections = true;
    }
    #endregion

    #region RoomRemoving
    private IEnumerator RemoveSmallestRooms()
    {
        float roomCountAtStart = rooms.adjacencyList.Count;
        float percentageRemoved = (1f - rooms.adjacencyList.Count / roomCountAtStart) * 100f;

        List<Room> roomList = rooms.KeysToList();

        // Sort the list
        if (roomSizeToBeRemoved == Sizes.Smallest)
            roomList = SortRoomsBySize(roomList, SortingOrders.SmallestToBiggest);
            
        else if (roomSizeToBeRemoved == Sizes.Biggest)
            roomList = SortRoomsBySize(roomList, SortingOrders.BiggestToSmallest);
            

        while (percentageRemoved < percentageOfRoomsToRemove)
        {
            // Remove first room in the sorted list
            Room roomToBeDestroyed = roomList[0];
            if (roomSizeToBeRemoved == Sizes.Random) roomToBeDestroyed = roomList[random.Next(0, roomList.Count)];

            List<Room> neighbors = rooms.GetNeighbors(roomToBeDestroyed);

            rooms.RemoveNode(roomToBeDestroyed);

            roomList.Remove(roomToBeDestroyed);

            if (AllRoomsAreConnected(roomList))
            {
                removedRooms.Add(roomToBeDestroyed);

                // Handle the percentage
                percentageRemoved = (1f - rooms.adjacencyList.Count / roomCountAtStart) * 100f;
            }

            else
            {
                rooms.AddNode(roomToBeDestroyed);
                for (int i = neighbors.Count - 1; i >= 0; i--)
                {
                    rooms.AddNeighbor(roomToBeDestroyed, neighbors[i]);
                }
                break;
            }

            // Visualization
            yield return new WaitForSeconds(timeBetweenSteps);
        }

        finishedRemoval = true;
    }
    private List<Room> SortRoomsBySize(List<Room> list, SortingOrders sortingOrder)
    {
        List<Room> sortedList = list;
        switch(sortingOrder)
        {
            case SortingOrders.SmallestToBiggest:
                // Complexity: O(n log n)
                sortedList = sortedList.OrderBy(t => t.size).ToList<Room>();
                break;
            case SortingOrders.BiggestToSmallest:
                // Complexity: O(n log n)
                sortedList = sortedList.OrderByDescending(t => t.size).ToList<Room>();
                break;
        }
        return sortedList;
    }
    private bool AllRoomsAreConnected(List<Room> list)
    {
        HashSet<Room> discovered = new HashSet<Room>();

        bool foundConnectedRooms = true;

        // Set the first to true
        discovered.Add(list[0]);

        // Keep looping through the list of rooms until no 'new' connected rooms are found
        while (foundConnectedRooms)
        {
            foundConnectedRooms = false;

            // Complexity (O(n^2))
            foreach (Room room in list)
            {
                foreach (Room neighbor in rooms.GetNeighbors(room))
                {
                    if (discovered.Contains(room) != discovered.Contains(neighbor))
                    {
                        foundConnectedRooms = true;
                        if (!discovered.Contains(room)) discovered.Add(room);
                        if (!discovered.Contains(neighbor)) discovered.Add(neighbor);
                    }
                }
            }
        }

        // Complexity: (O(n))
        foreach (Room room in list)
        {
            // If one room is not connected the dungeon is 'split' in 2
            if (!discovered.Contains(room))
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region PathGeneration
    private void GeneratePath()
    {
        switch (pathSearchingMethod)
        {
            case PathSearchingMethods.DepthFirst:
                rooms.DFS(rooms.adjacencyList.Keys.First());
                break;
            case PathSearchingMethods.BreadthFirst:
                rooms.BFS(rooms.adjacencyList.Keys.First());
                break;
        }
        finishedPathing = true;
    }
    #endregion

    #region DoorGenerating
    private IEnumerator GenerateDoors()
    {
        // For each room, check each neighboring room
        // Complexity: (O(n^2))
        foreach (Room room in rooms.adjacencyList.Keys)
        {
            for (int i = rooms.adjacencyList[room].Count - 1; i >= 0; i--)
            {
                List<Room> neighbors = rooms.adjacencyList[room];
                Room neighbor = neighbors[i];
                // if ONE of the rooms has no doors yet and the rooms are next to eachother, place a door at a random location
                if (!neighbor.isDoor)
                {
                    RectInt intersection = AlgorithmsUtils.Intersect(room.area, neighbor.area);
                    if (intersection.width > intersection.height)
                    {
                        // Create door
                        int pos = random.Next(intersection.xMin + wallBuffer, intersection.xMax - wallBuffer - doorSize + 1);
                        Room door = new Room();
                        door.area = new RectInt(pos, intersection.y, doorSize, intersection.height);
                        door.isDoor = true;

                        if (makeNonSquareRooms && room.area.width != neighbor.area.width)
                        {
                            if (random.Next(0, 101) <= chanceToMakeInterestingRoom * 100)
                            {
                                door.area = new RectInt(intersection.x + wallBuffer, intersection.y, intersection.width - wallBuffer * 2, intersection.height);
                            }
                        }

                        // Add door to list and as neighbors from rooms
                        doors.AddNode(door);

                        doors.AddNeighbor(door, room);
                        doors.AddNeighbor(door, neighbor);

                        rooms.AddNeighbor(room, door);
                        rooms.AddNeighbor(neighbor, door);

                        // Remove the connection from room to room
                        rooms.RemoveNeighbor(room, neighbor);
                        rooms.RemoveNeighbor(neighbor, room);

                        yield return new WaitForSeconds(timeBetweenSteps);
                    }

                    else
                    {
                        // Create door
                        int pos = random.Next(intersection.yMin + wallBuffer, intersection.yMax - wallBuffer - doorSize + 1);
                        Room door = new Room();
                        door.area = new RectInt(intersection.x, pos, intersection.width, doorSize);
                        door.isDoor = true;

                        if (makeNonSquareRooms && room.area.height != neighbor.area.height)
                        {
                            if (random.Next(0, 101) <= chanceToMakeInterestingRoom * 100)
                            {
                                door.area = new RectInt(intersection.x, intersection.y + wallBuffer, intersection.width, intersection.height - wallBuffer * 2);
                            }
                        }

                        // Add door to list and as neighbors from rooms
                        doors.AddNode(door);
                        doors.AddNeighbor(door, room);
                        doors.AddNeighbor(door, neighbor);

                        // Remove the connection from room to room
                        rooms.AddNeighbor(room, door);
                        rooms.AddNeighbor(neighbor, door);

                        // Remove the connection from room to room
                        rooms.RemoveNeighbor(room, neighbor);
                        rooms.RemoveNeighbor(neighbor, room);

                        yield return new WaitForSeconds(timeBetweenSteps);
                    }
                }
            }
        }

        finishedDoors = true;
    }
    #endregion

    #region DungeonDrawing
    void Update()
    {
        // Draw existing rooms in green
        if (showRooms && rooms.adjacencyList.Count > 0)
        {
            foreach (Room room in rooms.adjacencyList.Keys)
            {
                AlgorithmsUtils.DebugRectInt(room.area, Color.green);
            }

            AlgorithmsUtils.DebugRectInt(rooms.adjacencyList.Keys.First().area, Color.cyan);
        }

        // Create node lines:
        if (showGraph)
        {
            foreach (Room room in rooms.KeysToList())
            {
                foreach (Room neighbor in rooms.GetNeighbors(room))
                {
                    Vector3 roomPos = new Vector3(
                        room.area.center.x,
                        0,
                        room.area.center.y
                    );

                    Vector3 neighborPos = new Vector3(
                        neighbor.area.center.x,
                        0,
                        neighbor.area.center.y
                    );

                    Debug.DrawLine(roomPos, neighborPos, Color.red);
                }
            }
        }

        if (showDoors)
        {
            // Draw doors in blue
            foreach (Room door in doors.adjacencyList.Keys)
            {
                AlgorithmsUtils.DebugRectInt(door.area, Color.blue);
            }
        }

        if (showRemovedRooms)
        {
            // Draw random removed rooms in red
            foreach (Room room in removedRooms)
            {
                AlgorithmsUtils.DebugRectInt(room.area, Color.red);
            }
        }

        // Draw dungeon in dark green
        if (showDungeonOutLine) AlgorithmsUtils.DebugRectInt(dungeon, Color.green * 0.4f);
    }
    #endregion

    #region DungeonGeneration
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void GenerateDungeon()
    {
        ResetDungeon();
        
        StartCoroutine(DungeonGeneration());
    }
    private IEnumerator DungeonGeneration()
    {
        // Split the dungeon rooms
        StartCoroutine(SplitRooms());

        // Find all connections in rooms
        yield return new WaitUntil(() => finishedSplitting);
        StartCoroutine(FindConnections());

        // Remove random rooms from the dungeon
        yield return new WaitUntil(() => finishedFindingConnections);

        // Remove rooms
        if (removeRooms)
        {
            StartCoroutine(RemoveSmallestRooms());

            // Generate a path through the dungeon
            yield return new WaitUntil(() => finishedRemoval);
        }

        // Remove cyclic paths
        if (removeCyclingPaths)
        {
            GeneratePath();

            // Create doors to connect rooms to eachother
            yield return new WaitUntil(() => finishedPathing);
        }
        
        StartCoroutine(GenerateDoors());

        // Make the dungeon physical
        yield return new WaitUntil(() => finishedDoors);
        GameObject.Find("PhysicalGenerator").GetComponent<DungeonVisualizing>().MakeDungeonPhysical(rooms, doors, timeBetweenSteps);
    }
    private void ResetDungeon()
    {
        StopAllCoroutines();

        GameObject.Find("PhysicalGenerator").GetComponent<DungeonVisualizing>().ClearDungeon();

        // Reset bools
        finishedSplitting = false;
        finishedFindingConnections = false;
        finishedRemoval = false;
        finishedPathing = false;
        finishedDoors = false;

        // Reset lists
        rooms = new Graph<Room>();
        doors = new Graph<Room>();
        removedRooms.Clear();

        // Random seed
        if (useSeed) random = new System.Random(seed);
        else
        {
            random = new System.Random(System.Environment.TickCount);
            seed = System.Environment.TickCount;
        }
        Debug.Log("Random Seed: " + seed);

        // Generate outer room (dungeon outlines)
        dungeon = new RectInt(0, 0, dungeonWidth, dungeonHeight);

        // Create the first room
        Room dungeonSizedRoom = new Room();
        dungeonSizedRoom.area = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        rooms.AddNode(dungeonSizedRoom);
        firstRoom = dungeonSizedRoom;
    }
    #endregion
}