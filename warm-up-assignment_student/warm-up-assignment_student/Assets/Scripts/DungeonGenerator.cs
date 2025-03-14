using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using System;
using UnityEngine.UIElements;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using RangeAttribute = UnityEngine.RangeAttribute;
public class DungeonGenerator : MonoBehaviour
{
    #region variables
    [Header("Generation Method")]
    [SerializeField] private float timeBetweenSteps = 0.1f;

    [Header("Seed")]
    [SerializeField] private bool useSeed = false;
    [ShowIf("useSeed")][SerializeField] private int seed = 1;

    [Header("Dungeon Stats")]
    [SerializeField] private int dungeonWidth = 1000;
    [SerializeField] private int dungeonHeight = 1000;

    [Header("Room Stats")]
    [SerializeField] private int roomMinWidth = 100;
    [SerializeField] private int roomMinHeight = 100;
    [SerializeField] private int wallBuffer = 2;

    [Header("Door Stats")]
    [SerializeField] private int doorSize = 6;

    [Header("Generation Stats")]
    [Range(0f, 1f)][SerializeField] private float percentageOfRoomsToRemove = 0.1f;
    public enum Sizes { Smallest, Biggest }
    [SerializeField] private Sizes roomSizeToBeRemoved = Sizes.Smallest;

    [Header("Visualization")]
    [SerializeField] private bool showRooms = true;
    [SerializeField] private bool showDoors = true;
    [SerializeField] private bool showDungeonOutLine = true;
    [SerializeField] private bool showRemovedRooms = true;

    // Not in inspector
    public class Room
    {
        public RectInt area;
        public bool isConnectedToDungeon = false;
        public bool hasDoorsPlaced = false;
        public int size;
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
        StopCoroutine(SplitRooms());
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
        StopCoroutine(FindConnections());
    }
    #endregion

    #region RoomRemoving
    private IEnumerator RemoveSmallestRooms()
    {
        float roomCountAtStart = rooms.adjacencyList.Count;
        float percentageRemoved = 1f - rooms.adjacencyList.Count / roomCountAtStart;

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

            List<Room> neighbors = rooms.GetNeighbors(roomToBeDestroyed);

            rooms.RemoveNode(roomToBeDestroyed);

            roomList.Remove(roomToBeDestroyed);

            Debug.Log("Checking!");

            if (AllRoomsAreConnected(roomList))
            {
                removedRooms.Add(roomToBeDestroyed);

                // Handle the percentage
                percentageRemoved = 1f - rooms.adjacencyList.Count / roomCountAtStart;
            }

            else
            {
                rooms.AddNode(roomToBeDestroyed);
                for (int i = neighbors.Count - 1; i >= 0; i++)
                {
                    rooms.AddNeighbor(roomToBeDestroyed, neighbors[i]);
                }
                break;
            }

            // Visualization
            yield return new WaitForSeconds(timeBetweenSteps);
        }

        Debug.Log("End");
        finishedRemoval = true;
        StopCoroutine(RemoveSmallestRooms());
    }
    private List<Room> SortRoomsBySize(List<Room> list, SortingOrders sortingOrder)
    {
        List<Room> sortedList = list;
        sortedList.Clear();

        // Complexity: (O(n^2))
        foreach (Room room in list)
        {
            if (sortedList.Count == 0) sortedList.Add(room);

            else
            {
                // complexity: (O(n))
                for (int i = 0; i < sortedList.Count; i++)
                {
                    if (sortingOrder == SortingOrders.SmallestToBiggest && room.size < sortedList[i].size)
                    {
                        sortedList.Insert(i, room);
                        break;
                    }

                    else if (sortingOrder == SortingOrders.BiggestToSmallest && room.size > sortedList[i].size)
                    {
                        sortedList.Insert(i, room);
                        break;
                    }

                    else if (i == sortedList.Count - 1)
                    {
                        sortedList.Add(room);
                        break;
                    }
                }
            }
        }

        return sortedList;
    }
    int hello = 0;
    private bool AllRoomsAreConnected(List<Room> list)
    {
        bool allConnected = true;
        // Reset rooms in list
        for (int r = 0; r < list.Count; r++)
        {
            list[r].isConnectedToDungeon = false;
        }

        bool foundConnectedRooms = true;

        // Set the first to true
        list[0].isConnectedToDungeon = true;

        // Keep looping through the list of rooms until no 'new' connected rooms are found
        while (foundConnectedRooms)
        {
            foundConnectedRooms = false;

            // Complexity (O(n^2))
            foreach (Room room in list)
            {
                foreach (Room neighbor in rooms.GetNeighbors(room))
                {
                    if (room.isConnectedToDungeon != neighbor.isConnectedToDungeon)
                    {
                        foundConnectedRooms = true;
                        neighbor.isConnectedToDungeon = true;
                        room.isConnectedToDungeon = true;
                    }
                }
            }
        }

        hello++;
        Debug.Log(hello);

        // Complexity: (O(n))
        foreach (Room room in list)
        {
            // If one room is not connected the dungeon is 'split' in 2
            if (!room.isConnectedToDungeon) allConnected = false;
        }

        return allConnected;
    }
    #endregion

    #region PathGeneration
    private IEnumerator GeneratePath()
    {
        rooms.DFS(rooms.adjacencyList.Keys.First());
        yield return new WaitForSeconds(timeBetweenSteps);

        finishedPathing = true;
        StopCoroutine(GeneratePath());
    }
    #endregion

    #region DoorGenerating
    private IEnumerator GenerateDoors()
    {
        rooms.adjacencyList.Keys.First().hasDoorsPlaced = true;
        bool placedDoors = true;

        // Go through the list of rooms until all rooms have a door
        while (placedDoors)
        {
            placedDoors = false;

            // For each room, check each neighboring room
            // Complexity: (O(n^2))
            foreach (Room room in rooms.adjacencyList.Keys)
            {
                foreach (Room neighbor in rooms.GetNeighbors(room))
                {
                    // if ONE of the rooms has no doors yet and the rooms are next to eachother, place a door at a random location
                    if (room.hasDoorsPlaced != neighbor.hasDoorsPlaced)
                    {
                        RectInt intersection = AlgorithmsUtils.Intersect(room.area, neighbor.area);
                        if (intersection.width > intersection.height)
                        {
                            int pos = random.Next(intersection.xMin + wallBuffer, intersection.xMax - wallBuffer - doorSize + 1);
                            Room door = new Room();
                            door.area = new RectInt(pos, intersection.y, doorSize, intersection.height);
                            doors.AddNode(door);
                            doors.AddNeighbor(door, room);
                            doors.AddNeighbor(door, neighbor);
                            room.hasDoorsPlaced = true;
                            neighbor.hasDoorsPlaced = true;
                            placedDoors = true;
                            yield return new WaitForSeconds(timeBetweenSteps);
                        }

                        else
                        {
                            int pos = random.Next(intersection.yMin + wallBuffer, intersection.yMax - wallBuffer - doorSize + 1);
                            Room door = new Room();
                            door.area = new RectInt(intersection.x, pos, intersection.width, doorSize);
                            doors.AddNode(door);
                            doors.AddNeighbor(door, room);
                            doors.AddNeighbor(door, neighbor);
                            room.hasDoorsPlaced = true;
                            neighbor.hasDoorsPlaced = true;
                            placedDoors = true;
                            yield return new WaitForSeconds(timeBetweenSteps);
                        }
                    }
                }
            }
        }
        finishedDoors = true;
        StopCoroutine(GenerateDoors());
    }
    #endregion

    #region DungeonDrawing
    void Update()
    {
        // Draw existing rooms in green
        if (showRooms)
        {
            foreach (Room room in rooms.adjacencyList.Keys)
            {
                if (room.hasDoorsPlaced) AlgorithmsUtils.DebugRectInt(room.area, Color.green);
                else AlgorithmsUtils.DebugRectInt(room.area, Color.cyan);
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

        // Create node lines:
        foreach(Room room in rooms.KeysToList())
        {
            foreach(Room neighbor in rooms.GetNeighbors(room))
            {
                Vector3 roomPos;
                roomPos.x = room.area.x + room.area.width / 2f;
                roomPos.y = 0.1f;
                roomPos.z = room.area.y + room.area.height / 2f;

                Vector3 neighborPos;
                neighborPos.x = neighbor.area.x + neighbor.area.width / 2f;
                neighborPos.y = 0.1f;
                neighborPos.z = neighbor.area.y + neighbor.area.height / 2f;

                Debug.DrawLine(roomPos, neighborPos, Color.red);
            }
        }
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
        Debug.Log("Finished Splitting");
        StartCoroutine(FindConnections());

        // Remove random rooms from the dungeon
        yield return new WaitUntil(() => finishedFindingConnections);
        Debug.Log("Finished Finding Connections");
        //StartCoroutine(RemoveSmallestRooms());
        finishedRemoval = true;

        // Generate a path through the dungeon
        yield return new WaitUntil(() => finishedRemoval);
        Debug.Log("Finished Removal");
        StartCoroutine(GeneratePath());

        // Create doors to connect rooms to eachother
        yield return new WaitUntil(() => finishedPathing);
        Debug.Log("Finished path creation");
        StartCoroutine(GenerateDoors());

        // Stop the coroutine
        yield return new WaitUntil(() => finishedDoors);
        Debug.Log("Finished Doors");
        StopCoroutine(DungeonGeneration());
    }
    private void ResetDungeon()
    {
        StopAllCoroutines();

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