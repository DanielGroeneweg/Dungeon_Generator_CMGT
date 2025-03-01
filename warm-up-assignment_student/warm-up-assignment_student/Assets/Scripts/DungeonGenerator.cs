using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using NaughtyAttributes;
using System;
public class DungeonGenerator : MonoBehaviour
{
    #region variables
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
    [SerializeField] private int minRoomsToBeRemoved = 5;
    [SerializeField] private int maxRoomsToBeRemoved = 10;
    [SerializeField][Range(0f, 1f)] private float maxPercentageOfRoomsToBeRemoved = 0.5f;
    public enum RoomTargets { SmallestRoom, LargestRoom, RandomRoom, MostSquareRoom, LeastSquareRoom, firstRoomInList, LastRoomInList, MostConnectedRooms }
    [SerializeField] private RoomTargets firstRoomTarget;

    [Header("Rooms")]
    [SerializeField] private List<Room> rooms;
    [Serializable]
    public class Room
    {
        public RectInt room;
        public bool isConnectedToDungeon = false;
    }

    // Not in inspector
    private RectInt dungeon;
    private Room firstRoom;
    private List<RectInt> doors;
    private List<RectInt> randomRemovedRooms;
    private List<RectInt> unconnectedRemovedRooms;
    #endregion
    private void Start()
    {
        dungeon = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        firstRoom = new Room();
        firstRoom.room = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        rooms = new List<Room>();
        randomRemovedRooms = new List<RectInt>();
        unconnectedRemovedRooms = new List<RectInt>();
        doors = new List<RectInt>();
    }

    #region RoomSplitting
    private void SplitRooms()
    {
        // go through the list of rooms splitAmount times
        for (int loop = 1; loop <= splitAmount; loop++)
        {
            for (int i = rooms.Count - 1; i >= 0; i--)
            {
                // there is a ChanceToSplitRoom chance that a room is going to be split, unless the minimum amount of rooms has not been reached yet
                if (Random.Range(0f, 1f) <= ChanceToSplitRoom || rooms.Count < minimumAmountOfRooms)
                {
                    // Choose how to split
                    if (Random.Range(0, 2) == 1)
                    {
                        // Split horizontally if the room will not become too small, otherwise try vertically
                        if (rooms[i].room.height / 2 > roomMinHeight) SplitHorizontally(rooms[i]);

                        else if (rooms[i].room.width / 2 > roomMinWidth) SplitVertically(rooms[i]);
                    }
                    else
                    {
                        // Split vertically if the room will not become too small, otherwise try horizontally
                        if (rooms[i].room.width / 2 > roomMinWidth) SplitVertically(rooms[i]);

                        else if (rooms[i].room.height / 2 > roomMinHeight) SplitHorizontally(rooms[i]);
                    }
                }
            }
        }
    }
    // Split room horizontally (reduce the y/height)
    private void SplitHorizontally(Room roomObject)
    {
        // some casting to make the script readable
        RectInt room = roomObject.room;

        // create 2 room sizes, randomly picked based on the size of the room being split
        int randomSize = Random.Range(roomMinHeight, room.height - roomMinHeight + 1);
        int restSize = room.height - randomSize;

        // adds the walls to the room size to ensure rooms can overlap
        randomSize += wallBuffer;
        restSize += wallBuffer;

        // Create two new RectInts for the split rooms
        RectInt roomTop = new RectInt(room.x, room.y + randomSize - wallBuffer * 2, room.width, restSize);
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
    // Split room vertically (reduce the x/width)
    private void SplitVertically(Room roomObject)
    {
        // some casting to make the script readable
        RectInt room = roomObject.room;

        // create 2 room sizes, randomly picked based on the size of the room being split
        int randomSize = Random.Range(roomMinWidth, room.width - roomMinWidth + 1);
        int restSize = room.width - randomSize;

        // adds the walls to the room size to ensure rooms can overlap
        randomSize += wallBuffer;
        restSize += wallBuffer;

        // Create two new RectInts for the split rooms
        RectInt roomLeft = new RectInt(room.x, room.y, randomSize, room.height);
        RectInt roomRight = new RectInt(room.x + randomSize - wallBuffer * 2, room.y, restSize, room.height);

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
    private void RemoveRandomRooms()
    {
        float roomCountAtStart = rooms.Count;
        float percentageRemoved = 1f - rooms.Count / roomCountAtStart;
        
        // Pick a random amount of rooms to be removed that lies between the minimum to be removed and maximum to be removed
        int toBeDestroyed = Random.Range(minRoomsToBeRemoved, maxRoomsToBeRemoved + 1);

        for (int i = 1; i <= toBeDestroyed; i++)
        {
            if (percentageRemoved < maxPercentageOfRoomsToBeRemoved)
            {
                // Pick a random room and remove it
                int index = Random.Range(0, rooms.Count);
                randomRemovedRooms.Add(rooms[index].room);
                rooms.RemoveAt(index);
                percentageRemoved = 1f - rooms.Count / roomCountAtStart;
            }
        }
    }
    // Modifies the rooms in the list so that all rooms connected have their bool set to true
    // Requires at least 1 room with the bool set to true already
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
                        foundConnectedRooms = true;
                        list[i].isConnectedToDungeon = true;
                        list[j].isConnectedToDungeon = true;
                    }
                }
            }
        }
    }
    // Modifies the given list by removing all rooms that have the bool set to false
    // Returns a list of all rooms that are removed
    private List<Room> RemoveUnconnectedRooms(List<Room> list)
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
    // Starts an algorithm to find the group of rooms that has the largest amount of rooms, then picks that one as the dungeon
    private void FindMostConnectedRooms()
    {
        // A list that has groups (lists) of connected rooms
        List<List<Room>> connectedRoomGroups = new List<List<Room>>();

        // A list that has every room that is not in a connected room group
        List<Room> remainingRooms = new List<Room>(rooms);

        // While there are still rooms in the list of unsorted rooms
        while (remainingRooms.Count > 0)
        {
            // Check which of those are connected to the first room
            List<Room> connectedRooms = CheckWhichRoomsAreConnected(remainingRooms);

            // Remove the unconnected rooms from that list and store those removed rooms in the remainingRooms list
            remainingRooms = RemoveUnconnectedRooms(connectedRooms);

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
                    unconnectedRemovedRooms.Add(connectedRoomGroups[i][j].room);
                    rooms.Remove(connectedRoomGroups[i][j]);
                }
            }
        }

        rooms = mostConnectedRoomList;
        firstRoom = rooms[0];
    }
    // Sets the bool to true for the first room in the list, then returns the list with bools set to true for all connected rooms
    private List<Room> CheckWhichRoomsAreConnected(List<Room> roomList)
    {
        roomList[0].isConnectedToDungeon = true;

        FindConnectedRooms(roomList);

        return (roomList);
    }
    #endregion

    #region DoorGenerating
    private void GenerateDoors()
    {
        // For each room, check for each other room if they overlap
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                if (AlgorithmsUtils.Intersects(rooms[i].room, rooms[j].room))
                {
                    RectInt intersection = AlgorithmsUtils.Intersect(rooms[i].room, rooms[j].room);
                    if (intersection.width > intersection.height)
                    {
                        if (intersection.width >= wallBuffer * 4 + doorSize)
                        {
                            int pos = Random.Range(intersection.xMin + wallBuffer * 2, intersection.xMax - wallBuffer * 2 - doorSize + 1);
                            doors.Add(new RectInt(pos, intersection.y, doorSize, intersection.height));
                        }
                    }

                    else if (intersection.height >= wallBuffer * 4 + doorSize)
                    {
                        int pos = Random.Range(intersection.yMin + wallBuffer * 2, intersection.yMax - wallBuffer * 2 - doorSize + 1);
                        doors.Add(new RectInt(intersection.x, pos, intersection.width, doorSize));
                    }
                }
            }
        }
    }
    #endregion

    #region DungeonDrawing
    void Update()
    {
        // Draw existing rooms in yellow
        foreach (Room room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room.room, Color.yellow);
        }

        // Draw doors in blue
        foreach (RectInt door in doors)
        {
            AlgorithmsUtils.DebugRectInt(door, Color.blue);
        }

        // Draw random removed rooms in red
        foreach (RectInt room in randomRemovedRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.red);
        }

        // Draw unconnected removed rooms in magenta
        foreach (RectInt room in unconnectedRemovedRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.magenta);
        }

        // Draw dungeon in dark green
        AlgorithmsUtils.DebugRectInt(dungeon, Color.green * 0.4f);

        // Draw the first room in green
        AlgorithmsUtils.DebugRectInt(firstRoom.room, Color.green);
    }
    #endregion

    #region DungeonGeneration
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void GenerateDungeon()
    {
        // Random seed
        if (useSeed) Random.InitState(seed);
        else
        {
            Random.InitState(System.Environment.TickCount);
            seed = System.Environment.TickCount;
        }
        //Debug.Log("Random Seed: " + seed);

        // Generate outer room (dungeon outlines)
        dungeon = new RectInt(0, 0, dungeonWidth, dungeonHeight);

        // Reset dungeon
        rooms.Clear();
        randomRemovedRooms.Clear();
        unconnectedRemovedRooms.Clear();
        doors.Clear();

        // Create the first room
        Room dungeonSizedRoom = new Room();
        dungeonSizedRoom.room = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        rooms.Add(dungeonSizedRoom);

        // Split the dungeon rooms
        SplitRooms();

        // Remove random rooms from the dungeon
        RemoveRandomRooms();

        // Determine the first room of the dungeon
        firstRoom = null;
        switch (firstRoomTarget)
        {
            case RoomTargets.SmallestRoom:
                firstRoom = SmallestRoom();
                break;
            case RoomTargets.LargestRoom:
                firstRoom = LargestRoom();
                break;
            case RoomTargets.RandomRoom:
                firstRoom = RandomRoom();
                break;
            case RoomTargets.MostSquareRoom:
                firstRoom = MostSquareRoom();
                break;
            case RoomTargets.LeastSquareRoom:
                firstRoom = LeastSquareRoom();
                break;
            case RoomTargets.firstRoomInList:
                firstRoom = FirstRoomInList();
                break;
            case RoomTargets.LastRoomInList:
                firstRoom = LastRoomInList();
                break;
            case RoomTargets.MostConnectedRooms:
                break;
        }

        if (firstRoomTarget == RoomTargets.MostConnectedRooms)
        {
            FindMostConnectedRooms();
        }

        else
        {
            firstRoom.isConnectedToDungeon = true;
            FindConnectedRooms(rooms);
            List<Room> unconnectedRooms = RemoveUnconnectedRooms(rooms);
            foreach (Room room in unconnectedRooms)
            {
                unconnectedRemovedRooms.Add(room.room);
            }
        }

        // Create doors to connect rooms to eachother
        GenerateDoors();
    }
    #endregion

    #region FirstRoom
    private Room SmallestRoom()
    {
        Room smallestRoom = new Room();
        smallestRoom.room = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        int size = smallestRoom.room.width * smallestRoom.room.height;
        foreach (Room room in rooms)
        {
            if (room.room.width * room.room.height < size)
            {
                size = room.room.width * room.room.height;
                smallestRoom = room;
            }
        }
        return smallestRoom;
    }
    private Room LargestRoom()
    {
        int size = 0;
        Room largestRoom = null;
        foreach (Room room in rooms)
        {
            if (room.room.width * room.room.height > size)
            {
                size = room.room.width * room.room.height;
                largestRoom = room;
            }
        }
        return largestRoom;
    }
    private Room RandomRoom()
    {
        return rooms[Random.Range(0, rooms.Count)];
    }
    private Room MostSquareRoom()
    {
        float ratioOffset = -1;
        Room mostSquareRoom = null;
        foreach (Room room in rooms)
        {
            float ratio = (float)Mathf.Min(room.room.width, room.room.height) / (float)Mathf.Max(room.room.width, room.room.height);
            if (Mathf.Abs(1 - ratio) < Mathf.Abs(1 - ratioOffset))
            {
                ratioOffset = ratio;
                mostSquareRoom = room;
            }
        }
        return mostSquareRoom;
    }
    private Room LeastSquareRoom()
    {
        float ratioOffset = 1;
        Room leastSquareRoom = null;
        foreach (Room room in rooms)
        {
            float ratio = (float)Mathf.Min(room.room.width, room.room.height) / (float)Mathf.Max(room.room.width, room.room.height);
            if (Mathf.Abs(1 - ratio) > Mathf.Abs(1 - ratioOffset))
            {
                ratioOffset = ratio;
                leastSquareRoom = room;
            }
        }
        return leastSquareRoom;
    }
    private Room FirstRoomInList()
    {
        return rooms[0];
    }
    private Room LastRoomInList()
    {
        return rooms[rooms.Count - 1];
    }
    #endregion
}