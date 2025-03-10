using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using System;
using UnityEngine.UIElements;
using System.Collections;
using System.Linq;
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
    [Range(0f, 1f)][SerializeField] private float minRoomsToBeRemovedPercentage = 0.1f;
    [Range(0f, 1f)][SerializeField] private float maxRoomsToBeRemovedPercentage = 0.5f;

    [Header("Visualization")]
    [SerializeField] private bool showRooms = true;
    [SerializeField] private bool showDoors = true;
    [SerializeField] private bool showFirstRoom = true;
    [SerializeField] private bool showDungeonOutLine = true;
    [SerializeField] private bool showRandomRemovedRooms = true;
    [SerializeField] private bool showDisconnectedRemovedRooms = true;

    // Not in inspector
    public Graph<Room> rooms;
    public Graph<Room> doors;
    public class Room
    {
        public RectInt area;
        public bool isConnectedToDungeon = false;
        public bool hasDoorsPlaced = false;
        public bool hasBeenVisited = false;
    }

    private RectInt dungeon;
    private Room firstRoom;
    private List<RectInt> randomRemovedRooms;
    private List<RectInt> disconnectedRemovedRooms;

    // Used for coroutines
    private bool finishedSplitting = false;
    private bool finishedRandomRemoval = false;
    private bool finishedRestRemoval = false;
    private bool finishedDoors = false;

    // Random
    private System.Random random;
    #endregion
    private void Start()
    {
        dungeon = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        firstRoom = new Room();
        firstRoom.area = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        rooms = new Graph<Room>();
        randomRemovedRooms = new List<RectInt>();
        disconnectedRemovedRooms = new List<RectInt>();
        doors = new Graph<Room>();
    }

    #region RoomSplitting
    // Coroutine for visualization
    private IEnumerator SplitRooms()
    {
        bool changedARoom = true;
        // go through the list of rooms splitAmount times
        while (changedARoom)
        {
            List<Room> unfinishedRooms = new List<Room>(rooms.adjacencyList.Keys);
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

        // Create a new Room object for the bottom room
        Room roomObjectBottom = new Room();
        roomObjectBottom.area = roomBottom;

        // Add the newly created rooms to the room list
        rooms.AddRoom(roomObjectTop);
        rooms.AddRoom(roomObjectBottom);

        // Remove the original room from the room list
        rooms.adjacencyList.Remove(roomObject);
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

        // Create a new Room object for the right room
        Room roomObjectRight = new Room();
        roomObjectRight.area = roomRight;

        // Add the newly created rooms to the room list
        rooms.AddRoom(roomObjectLeft);
        rooms.AddRoom(roomObjectRight);

        // Remove the original room from the room list
        rooms.adjacencyList.Remove(roomObject);
    }
    #endregion

    #region RoomRemoving
    #region Random
    private IEnumerator RemoveRandomRooms()
    {
        float roomCountAtStart = rooms.adjacencyList.Count;
        float percentageRemoved = 1f - rooms.adjacencyList.Count / roomCountAtStart;

        // Pick a random amount of rooms to be removed that lies between the minimum to be removed and maximum to be removed
        float toBeDestroyed = random.Next((int)(minRoomsToBeRemovedPercentage * 100), (int)(maxRoomsToBeRemovedPercentage * 100) + 1) / 100f;

        List<Room> roomList = new List<Room>(rooms.adjacencyList.Keys);

        while (percentageRemoved < toBeDestroyed)
        {
            // Pick a random room and remove it
            int index = random.Next(0, rooms.adjacencyList.Count);
            Room roomToBeDestroyed = roomList[index];
            randomRemovedRooms.Add(roomToBeDestroyed.area);
            rooms.adjacencyList.Remove(roomToBeDestroyed);
            percentageRemoved = 1f - rooms.adjacencyList.Count / roomCountAtStart;
            yield return new WaitForSeconds(timeBetweenSteps);
        }

        finishedRandomRemoval = true;
        StopCoroutine(RemoveRandomRooms());
    }
    #endregion

    #region Disconnected
    /// <summary>
    /// Starts an algorithm to find the group of rooms that has the largest amount of rooms, then picks that one as the dungeon
    /// </summary>
    /// <returns></returns>
    private IEnumerator FindMostConnectedRooms()
    {
        List<List<Room>> connectedRoomGroups = new List<List<Room>>();

        List<Room> remainingRooms = new List<Room>(rooms.adjacencyList.Keys);

        // While there are still rooms in the list of unsorted rooms
        while (remainingRooms.Count > 0)
        {
            // Check which of those are connected to the first room
            List<Room> connectedRooms = CheckWhichRoomsAreConnected(remainingRooms);

            // Remove the unconnected rooms from that list and store those removed rooms in the remainingRooms list
            remainingRooms = RemoveDisconnectedRooms(connectedRooms);

            // Add the connected rooms as a group to the list of connected room groups
            connectedRoomGroups.Add(connectedRooms);
        }

        // Check which group has the most connected rooms
        int highestRoomCount = 0;
        List<Room> mostConnectedRoomList = new List<Room>();
        foreach (List<Room> connectedRoomGroup in connectedRoomGroups)
        {
            if (connectedRoomGroup.Count > highestRoomCount)
            {
                highestRoomCount = connectedRoomGroup.Count;
                mostConnectedRoomList = connectedRoomGroup;
            }
        }

        // Remove all rooms from the original rooms list that are not in the most connected group
        for (int i = connectedRoomGroups.Count - 1; i >= 0; i--)
        {
            if (connectedRoomGroups[i] != mostConnectedRoomList)
            {
                for (int j = connectedRoomGroups[i].Count - 1; j >= 0; j--)
                {
                    disconnectedRemovedRooms.Add(connectedRoomGroups[i][j].area);
                    rooms.adjacencyList.Remove(connectedRoomGroups[i][j]);
                    yield return new WaitForSeconds(timeBetweenSteps);
                }
            }
        }

        firstRoom = rooms.adjacencyList.Keys.First();

        finishedRestRemoval = true;
        StopCoroutine(FindMostConnectedRooms());
    }
    /// <summary>
    /// Modifies the given list by removing all rooms that have the bool set to false.
    /// Returns a list of all rooms that are removed
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private List<Room> RemoveDisconnectedRooms(List<Room> list)
    {
        List<Room> removedRooms = new List<Room>();
        // Inverse Loop to remove all rooms not connected in any way to the picked room
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (!list[i].isConnectedToDungeon)
            {
                removedRooms.Add(list[i]);
                list.RemoveAt(i);
            }
        }

        return removedRooms;
    }
    /// <summary>
    /// Sets the bool to true for the first room in the list, then returns the list with bools set to true for all connected rooms
    /// </summary>
    /// <param name="roomList"></param>
    /// <returns></returns>
    private List<Room> CheckWhichRoomsAreConnected(List<Room> roomList)
    {
        roomList[0].isConnectedToDungeon = true;

        FindConnectedRooms(roomList);

        return (roomList);
    }
    /// <summary>
    /// Modifies the rooms in the list so that all rooms connected have their bool set to true.
    /// Requires at least 1 room with the bool set to true already
    /// </summary>
    /// <param name="list"></param>

    private void FindConnectedRooms(List<Room> list)
    {
        bool foundConnectedRooms = true;

        // Keep looping through the list of rooms until no 'new' connected rooms are found
        while (foundConnectedRooms)
        {
            foundConnectedRooms = false;

            // For each room check each other room
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    // if only one of the rooms is connected to the dungeon and the rooms overlap, set both rooms to being connected
                    if (list[i].isConnectedToDungeon != list[j].isConnectedToDungeon && AlgorithmsUtils.Intersects(list[i].area, list[j].area))
                    {
                        RectInt intersect = AlgorithmsUtils.Intersect(list[i].area, list[j].area);
                        if (intersect.width >= wallBuffer * 2 + doorSize || intersect.height >= wallBuffer * 2 + doorSize)
                        {
                            foundConnectedRooms = true;
                            rooms.AddNeighbor(list[i], list[j]);
                            list[i].isConnectedToDungeon = true;
                            list[j].isConnectedToDungeon = true;
                        }
                    }
                }
            }
        }
    }
    #endregion
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

            // For each room, check each other room
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
                            doors.AddRoom(door);
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
                            doors.AddRoom(door);
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
        // While Running, fix the maxRoomsToBeDestroyed
        if (maxRoomsToBeRemovedPercentage < minRoomsToBeRemovedPercentage) maxRoomsToBeRemovedPercentage = minRoomsToBeRemovedPercentage;

        // Draw existing rooms in green
        if (showRooms)
        {
            foreach (Room room in rooms.adjacencyList.Keys)
            {
                AlgorithmsUtils.DebugRectInt(room.area, Color.green);
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

        if (showRandomRemovedRooms)
        {
            // Draw random removed rooms in red
            foreach (RectInt room in randomRemovedRooms)
            {
                AlgorithmsUtils.DebugRectInt(room, Color.red);
            }
        }

        if (showDisconnectedRemovedRooms)
        {
            // Draw unconnected removed rooms in magenta
            foreach (RectInt room in disconnectedRemovedRooms)
            {
                AlgorithmsUtils.DebugRectInt(room, Color.magenta);
            }
        }

        // Draw dungeon in dark green
        if (showDungeonOutLine) AlgorithmsUtils.DebugRectInt(dungeon, Color.green * 0.4f);

        // Draw the first room in cyan
        if (showFirstRoom) AlgorithmsUtils.DebugRectInt(firstRoom.area, Color.cyan);
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

        // Remove random rooms from the dungeon
        yield return new WaitUntil(() => finishedSplitting);
        StartCoroutine(RemoveRandomRooms());

        // Remove unconnected rooms from the dungeon
        yield return new WaitUntil(() => finishedRandomRemoval);
        StartCoroutine(FindMostConnectedRooms());

        // Create doors to connect rooms to eachother
        yield return new WaitUntil(() => finishedRestRemoval);
        StartCoroutine(GenerateDoors());

        // Stop the coroutine
        yield return new WaitUntil(() => finishedDoors);
        StopCoroutine(DungeonGeneration());
    }
    private void ResetDungeon()
    {
        StopAllCoroutines();

        // Reset bools
        finishedSplitting = false;
        finishedRandomRemoval = false;
        finishedRestRemoval = false;
        finishedDoors = false;

        // Reset lists
        rooms = new Graph<Room>();
        doors = new Graph<Room>();
        randomRemovedRooms.Clear();
        disconnectedRemovedRooms.Clear();

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
        rooms.AddRoom(dungeonSizedRoom);
        firstRoom = dungeonSizedRoom;
    }
    #endregion
}