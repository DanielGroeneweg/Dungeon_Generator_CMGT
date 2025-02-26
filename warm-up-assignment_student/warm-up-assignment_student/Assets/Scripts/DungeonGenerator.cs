using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using NaughtyAttributes;

public class DungeonGenerator : MonoBehaviour
{
    #region variables
    [Header("Dungeon Stats")]
    [SerializeField] private int dungeonWidth = 1000;
    [SerializeField] private int dungeonLength = 1000;

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

    [Header("Rooms")]
    [SerializeField] private List<RectInt> rooms;
    [SerializeField] private List<RectInt> doors;

    // Not in inspector
    private RectInt dungeon;

    private List<RectInt> removedRooms;
    #endregion
    private void Start()
    {
        dungeon = new RectInt(0, 0, dungeonWidth, dungeonLength);
        rooms = new List<RectInt>();
        removedRooms = new List<RectInt>();
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
                        if (rooms[i].height / 2 > roomMinHeight) SplitHorizontally(rooms[i]);

                        else if (rooms[i].width / 2 > roomMinWidth) SplitVertically(rooms[i]);
                    }
                    else
                    {
                        // Split vertically if the room will not become too small, otherwise try horizontally
                        if (rooms[i].width / 2 > roomMinWidth) SplitVertically(rooms[i]);

                        else if (rooms[i].height / 2 > roomMinHeight) SplitHorizontally(rooms[i]);
                    }
                }
            }
        }
    }
    // Split room horizontally (reduce the y/height)
    private void SplitHorizontally(RectInt room)
    {
        int randomSize = Random.Range(roomMinHeight, room.height - roomMinHeight + 1);
        int restSize = room.height - randomSize;

        randomSize += wallBuffer;
        restSize += wallBuffer;

        RectInt roomTop = new RectInt(room.x, room.y + randomSize - wallBuffer * 2, room.width, restSize);
        RectInt roomBottom = new RectInt(room.x, room.y, room.width, randomSize);

        rooms.Add(roomTop);
        rooms.Add(roomBottom);

        rooms.Remove(room);
    }
    // Split room vertically (reduce the x/width)
    private void SplitVertically(RectInt room)
    {
        int randomSize = Random.Range(roomMinWidth, room.width - roomMinWidth + 1);
        int restSize = room.width - randomSize;

        randomSize += wallBuffer;
        restSize += wallBuffer;

        RectInt roomLeft = new RectInt(room.x, room.y, randomSize, room.height);
        RectInt roomRight = new RectInt(room.x + randomSize - wallBuffer * 2, room.y, restSize, room.height);

        rooms.Add(roomLeft);
        rooms.Add(roomRight);

        rooms.Remove(room);
    }
    #endregion

    #region RandomRoomRemoving
    private void RemoveRandomRooms()
    {
        // Pick a random amount of rooms to be removed that lies between the minimum to be removed and maximum to be removed
        int toBeDestroyed = Random.Range(minRoomsToBeRemoved, maxRoomsToBeRemoved + 1);
        for (int i = 1; i <= toBeDestroyed; i++)
        {
            // Make sure there is always at least 1 room
            if (rooms.Count > minimumAmountOfRooms)
            {
                // Pick a random room and remove it
                int index = Random.Range(0, rooms.Count);
                removedRooms.Add(rooms[index]);
                rooms.RemoveAt(index);
            }
        }
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
                if (AlgorithmsUtils.Intersects(rooms[i], rooms[j]))
                {
                    RectInt intersection = AlgorithmsUtils.Intersect(rooms[i], rooms[j]);
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
        foreach (RectInt room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.yellow);
        }

        // Draw doors in blue
        foreach (RectInt door in doors)
        {
            AlgorithmsUtils.DebugRectInt(door, Color.blue);
        }

        // Draw dungeon in green
        AlgorithmsUtils.DebugRectInt(dungeon, Color.green);

        // Draw removed rooms in red
        foreach (RectInt room in removedRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.red);
        }
    }
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void GenerateDungeon()
    {
        // Generate outer room (dungeon outlines)
        dungeon = new RectInt(0, 0, dungeonWidth, dungeonLength);

        // Reset dungeon
        rooms.Clear();
        removedRooms.Clear();
        doors.Clear();

        // Create new dungeon
        rooms.Add(new RectInt(0, 0, dungeonWidth, dungeonLength));
        SplitRooms();
        RemoveRandomRooms();
        GenerateDoors();
    }
    #endregion
}