using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using NaughtyAttributes;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Stats")]
    [SerializeField] private int dungeonWidth = 1000;
    [SerializeField] private int dungeonLength = 1000;

    [Header("Room Stats")]
    [SerializeField] private int roomMinWidth = 100;
    [SerializeField] private int roomMinLength = 100;
    [SerializeField] private int roomHeight = 50;
    [SerializeField] [Range(0f, 1f)] private float ChanceToSplitRoom;

    [Header("Generation Stats")]
    [SerializeField] private int splitAmount = 10;

    private RectInt dungeon;
    [Header("Rooms")]
    [SerializeField] private List<RectInt> rooms;
    private void SplitRooms()
    {
        for (int loop = 1; loop <= splitAmount; loop++)
        {
            for (int i = rooms.Count - 1; i >= 0; i--)
            {
                if (Random.Range(0f, 1f) <= ChanceToSplitRoom)
                {
                    Debug.Log("Splitting!");

                    // Choose how to split
                    if (Random.Range(0, 2) == 1)
                    {
                        // Split horizontally if the room will not become too small, otherwise try vertically
                        if (rooms[i].height / 2 > roomMinLength) SplitHorizontally(rooms[i]);

                        else if (rooms[i].width / 2 > roomMinWidth) SplitVertically(rooms[i]);
                    }
                    else
                    {
                        // Split vertically if the room will not become too small, otherwise try horizontally
                        if (rooms[i].width > roomMinWidth) SplitVertically(rooms[i]);

                        else if (rooms[i].height / 2 > roomMinLength) SplitHorizontally(rooms[i]);
                    }
                }
                else Debug.Log("Did not split!");
            }
        }
    }
    private void SplitHorizontally(RectInt room)
    {
        RectInt roomTop = new RectInt(room.x, room.y + room.height / 2, room.width, room.height / 2);
        RectInt roomBottom = new RectInt(room.x, room.y, room.width, room.height / 2);

        rooms.Add(roomTop);
        rooms.Add(roomBottom);

        rooms.Remove(room);
    }
    private void SplitVertically(RectInt room)
    {
        RectInt roomLeft = new RectInt(room.x, room.y, room.width / 2, room.height);
        RectInt roomRight = new RectInt(room.x + room.width / 2, room.y, room.width / 2, room.height);

        rooms.Add(roomLeft);
        rooms.Add(roomRight);

        rooms.Remove(room);
    }
    void Update()
    {
        foreach (RectInt room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.yellow);
        }

        AlgorithmsUtils.DebugRectInt(dungeon, Color.red);
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void GenerateDungeon()
    {
        // Generate outer room (dungeon outlines)
        dungeon = new RectInt(0, 0, dungeonWidth, dungeonLength);

        // Reset dungeon
        rooms = new List<RectInt>();

        // Create new dungeon
        rooms.Add(new RectInt(0, 0, dungeonWidth, dungeonLength));
        SplitRooms();
    }
}