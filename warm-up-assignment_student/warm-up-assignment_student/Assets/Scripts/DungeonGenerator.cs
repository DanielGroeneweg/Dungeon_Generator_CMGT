using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using System;
using UnityEngine.UIElements;
using System.Collections;
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
    [SerializeField] private int minimumAmountOfRooms = 5;
    [SerializeField] private int splitAmount = 10;
    [SerializeField][Range(0f, 1f)] private float ChanceToSplitRoom = 0.5f;
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
    public List<Room> rooms;
    public class Room
    {
        public RectInt room;
        public bool isConnectedToDungeon = false;
        public bool hasDoorsPlaced = false;
        //Dictionary<int, Room> adjacentRooms = new Dictionary<int, Room>();
    }

    private RectInt dungeon;
    private Room firstRoom;
    private List<RectInt> doors;
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
        firstRoom.room = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        rooms = new List<Room>();
        randomRemovedRooms = new List<RectInt>();
        disconnectedRemovedRooms = new List<RectInt>();
        doors = new List<RectInt>();
    }

    #region RoomSplitting
    // Coroutine for visualization
    private IEnumerator SplitRooms()
    {
        // go through the list of rooms splitAmount times
        for (int loop = 1; loop <= splitAmount; loop++)
        {
            for (int i = rooms.Count - 1; i >= 0; i--)
            {
                // there is a ChanceToSplitRoom chance that a room is going to be split, unless the minimum amount of rooms has not been reached yet
                if (random.Next(0, 101) <= ChanceToSplitRoom * 100f || rooms.Count < minimumAmountOfRooms)
                {
                    // Choose how to split
                    if (random.Next(0, 2) == 1)
                    {
                        // Split horizontally if the room will not become too small, otherwise try vertically
                        if (rooms[i].room.height / 2 > roomMinHeight)
                        {
                            SplitHorizontally(rooms[i]);
                            yield return new WaitForSeconds(timeBetweenSteps);
                        }

                        else if (rooms[i].room.width / 2 > roomMinWidth)
                        {
                            SplitVertically(rooms[i]);
                            yield return new WaitForSeconds(timeBetweenSteps);
                        }
                    }
                    else
                    {
                        // Split vertically if the room will not become too small, otherwise try horizontally
                        if (rooms[i].room.width / 2 > roomMinWidth)
                        {
                            SplitVertically(rooms[i]);
                            yield return new WaitForSeconds(timeBetweenSteps);
                        }

                        else if (rooms[i].room.height / 2 > roomMinHeight)
                        {
                            SplitHorizontally(rooms[i]);
                            yield return new WaitForSeconds(timeBetweenSteps);
                        }
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
        RectInt room = roomObject.room;

        // create 2 room sizes, randomly picked based on the size of the room being split
        int randomSize = random.Next(roomMinHeight, room.height - roomMinHeight + 1);
        int restSize = room.height - randomSize;

        // Create two new RectInts for the split rooms
        RectInt roomTop = new RectInt(room.x, room.y + randomSize - wallBuffer, room.width, restSize + wallBuffer);
        RectInt roomBottom = new RectInt(room.x, room.y, room.width, randomSize);

        // Create a new Room object for the top room
        Room roomObjectTop = new Room();
        roomObjectTop.room = roomTop;

        // Create a new Room object for the bottom room
        Room roomObjectBottom = new Room();
        roomObjectBottom.room = roomBottom;

        // Add the newly created rooms to the room list
        rooms.Add(roomObjectTop);
        rooms.Add(roomObjectBottom);

        // Remove the original room from the room list
        rooms.Remove(roomObject);
    }
    /// <summary>
    /// Split room vertically (reduce the x/width)
    /// </summary>
    /// <param name="roomObject"></param>
    private void SplitVertically(Room roomObject)
    {
        // some casting to make the script readable
        RectInt room = roomObject.room;

        // create 2 room sizes, randomly picked based on the size of the room being split
        int randomSize = random.Next(roomMinWidth, room.width - roomMinWidth + 1);
        int restSize = room.width - randomSize;

        // Create two new RectInts for the split rooms
        RectInt roomLeft = new RectInt(room.x, room.y, randomSize, room.height);
        RectInt roomRight = new RectInt(room.x + randomSize - wallBuffer, room.y, restSize + wallBuffer, room.height);

        // Create a new Room object for the left room
        Room roomObjectLeft = new Room();
        roomObjectLeft.room = roomLeft;

        // Create a new Room object for the right room
        Room roomObjectRight = new Room();
        roomObjectRight.room = roomRight;

        // Add the newly created rooms to the room list
        rooms.Add(roomObjectLeft);
        rooms.Add(roomObjectRight);

        // Remove the original room from the room list
        rooms.Remove(roomObject);
    }
    #endregion

    #region RoomRemoving
    #region Random
    private IEnumerator RemoveRandomRooms()
    {
        float roomCountAtStart = rooms.Count;
        float percentageRemoved = 1f - rooms.Count / roomCountAtStart;

        // Pick a random amount of rooms to be removed that lies between the minimum to be removed and maximum to be removed
        float toBeDestroyed = random.Next((int)(minRoomsToBeRemovedPercentage * 100), (int)(maxRoomsToBeRemovedPercentage * 100) + 1) / 100f;

        while (percentageRemoved < toBeDestroyed)
        {
            // Pick a random room and remove it
            int index = random.Next(0, rooms.Count);
            randomRemovedRooms.Add(rooms[index].room);
            rooms.RemoveAt(index);
            percentageRemoved = 1f - rooms.Count / roomCountAtStart;
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

        List<Room> remainingRooms = new List<Room>(rooms);

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
                    disconnectedRemovedRooms.Add(connectedRoomGroups[i][j].room);
                    rooms.Remove(connectedRoomGroups[i][j]);
                    yield return new WaitForSeconds(timeBetweenSteps);
                }
            }
        }

        rooms = mostConnectedRoomList;
        firstRoom = rooms[0];

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
                    if (list[i].isConnectedToDungeon != list[j].isConnectedToDungeon && AlgorithmsUtils.Intersects(list[i].room, list[j].room))
                    {
                        RectInt intersect = AlgorithmsUtils.Intersect(list[i].room, list[j].room);
                        if (intersect.width >= wallBuffer * 2 + doorSize || intersect.height >= wallBuffer * 2 + doorSize)
                        {
                            foundConnectedRooms = true;
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
        rooms[0].hasDoorsPlaced = true;
        bool placedDoors = true;

        // Go through the list of rooms until all rooms have a door
        while (placedDoors)
        {
            placedDoors = false;

            // For each room, check each other room
            for (int i = 0; i < rooms.Count; i++)
            {
                for (int j = i + 1; j < rooms.Count; j++)
                {
                    // if ONE of the rooms has no doors yet and the rooms are next to eachother, place a door at a random location
                    if (rooms[i].hasDoorsPlaced != rooms[j].hasDoorsPlaced && AlgorithmsUtils.Intersects(rooms[i].room, rooms[j].room))
                    {
                        RectInt intersection = AlgorithmsUtils.Intersect(rooms[i].room, rooms[j].room);
                        if (intersection.width > intersection.height)
                        {
                            if (intersection.width >= wallBuffer * 2 + doorSize)
                            {
                                int pos = random.Next(intersection.xMin + wallBuffer, intersection.xMax - wallBuffer - doorSize + 1);
                                doors.Add(new RectInt(pos, intersection.y, doorSize, intersection.height));
                                rooms[i].hasDoorsPlaced = true;
                                rooms[j].hasDoorsPlaced = true;
                                placedDoors = true;
                                yield return new WaitForSeconds(timeBetweenSteps);
                            }
                        }

                        else if (intersection.height >= wallBuffer * 2 + doorSize)
                        {
                            int pos = random.Next(intersection.yMin + wallBuffer, intersection.yMax - wallBuffer - doorSize + 1);
                            doors.Add(new RectInt(intersection.x, pos, intersection.width, doorSize));
                            rooms[i].hasDoorsPlaced = true;
                            rooms[j].hasDoorsPlaced = true;
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

        // Draw existing rooms in yellow
        if (showRooms)
        {
            foreach (Room room in rooms)
            {
                AlgorithmsUtils.DebugRectInt(room.room, Color.yellow);
            }
        }

        if (showDoors)
        {
            // Draw doors in blue
            foreach (RectInt door in doors)
            {
                AlgorithmsUtils.DebugRectInt(door, Color.blue);
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

        // Draw the first room in green
        if (showFirstRoom) AlgorithmsUtils.DebugRectInt(firstRoom.room, Color.green);
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
        rooms.Clear();
        randomRemovedRooms.Clear();
        disconnectedRemovedRooms.Clear();
        doors.Clear();

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
        dungeonSizedRoom.room = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        rooms.Add(dungeonSizedRoom);
        firstRoom = dungeonSizedRoom;
    }
    #endregion
}