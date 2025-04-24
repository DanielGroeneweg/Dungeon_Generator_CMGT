using System;
using System.Text;
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
[DefaultExecutionOrder(3)]
public class TileMapGenerator : MonoBehaviour
{
    [DoNotSerialize] public int[,] _tileMap;
    public void GenerateTileMap(RectInt dungeonBounds, List<DungeonGenerator.Room> rooms, List<DungeonGenerator.Room> doors)
    {
        int[,] tileMap = new int[dungeonBounds.height, dungeonBounds.width];
        int rows = tileMap.GetLength(0);
        int cols = tileMap.GetLength(1);

        //Fill the map with empty spaces

        //Draw the rooms
        foreach (DungeonGenerator.Room room in rooms)
        {
            AlgorithmsUtils.FillRectangleOutline(tileMap, room.area, 1);
        }

        //Draw the doors
        foreach (DungeonGenerator.Room door in doors)
        {
            AlgorithmsUtils.FillRectangleOutline(tileMap, door.area, 0);
            AlgorithmsUtils.FillRectangle(tileMap, door.area, 0);
        }

        _tileMap = tileMap;
    }
    public string ToString(bool flip)
    {
        if (_tileMap == null) return "Tile map not generated yet.";

        int rows = _tileMap.GetLength(0);
        int cols = _tileMap.GetLength(1);

        var sb = new StringBuilder();

        int start = flip ? rows - 1 : 0;
        int end = flip ? -1 : rows;
        int step = flip ? -1 : 1;

        for (int i = start; i != end; i += step)
        {
            for (int j = 0; j < cols; j++)
            {
                sb.Append((_tileMap[i, j] == 0 ? '0' : '#')); //Replaces 1 with '#' making it easier to visualize
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
    public int[,] GetTileMap()
    {
        return _tileMap.Clone() as int[,];
    }
    [Button]
    public void PrintTileMap()
    {
        Debug.Log(ToString(true));
    }
}