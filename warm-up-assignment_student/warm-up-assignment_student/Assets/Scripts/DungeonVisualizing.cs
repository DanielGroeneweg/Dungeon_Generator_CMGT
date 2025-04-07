using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using System;
using UnityEngine.UIElements;
using System.Collections;
using System.Linq;
using RangeAttribute = UnityEngine.RangeAttribute;
public class DungeonVisualizing : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Transform floorPrefab;
    [SerializeField] private Transform wallPrefab;

    [Header("Stats")]
    [SerializeField] private float wallHeight = 4;

    private Graph<DungeonGenerator.Room> rooms;
    private Graph<DungeonGenerator.Room> doors;

    // bools for visualization
    bool madeFloor = false;
    bool madeWalls = false;

    private float TimeBetweenSteps = 0;
    public void MakeDungeonPhysical(Graph<DungeonGenerator.Room> roomGraph, Graph<DungeonGenerator.Room> doorGraph, float time)
    {
        TimeBetweenSteps = time;

        madeFloor = false;
        madeWalls = false;

        rooms = roomGraph;
        doors = doorGraph;

        StartCoroutine(Generate());
    }
    public void ClearDungeon()
    {
        StopAllCoroutines();

        Transform[] array = GetComponentsInChildren<Transform>();
        List<Transform> list = new List<Transform>();

        foreach(Transform obj in array)
        {
            list.Add(obj);
        }

        for (int i = list.Count - 1; i >= 0; i--)
        {
            Transform obj = list[i];
            
            // Do not destroy this object
            if (obj != transform)
            {
                Destroy(obj.gameObject);
                list.Remove(obj);
            }
        }
    }
    private IEnumerator Generate()
    {
        StartCoroutine(GenerateFloor());
        yield return new WaitUntil(() => madeFloor);

        GenerateWalls();
    }
    private IEnumerator GenerateFloor()
    {
        foreach (DungeonGenerator.Room room in rooms.KeysToList())
        {
            Transform obj = GameObject.Instantiate(
                floorPrefab,
                new Vector3(room.area.x + room.area.width / 2f, 0, room.area.y + room.area.height / 2f),
                Quaternion.identity, transform
                );

            obj.localScale = new Vector3(room.area.width, 0.1f, room.area.height);

            yield return new WaitForSeconds(TimeBetweenSteps);
        }
        madeFloor = true;
    }
    private void GenerateWalls()
    {
        foreach(DungeonGenerator.Room room in rooms.KeysToList())
        {
            StartCoroutine(MakeWalls(room));
        }
    }
    private IEnumerator MakeWalls(DungeonGenerator.Room room)
    {
        foreach (Vector2 position in room.area.allPositionsWithin)
        {
            if (position.x == room.area.xMin || position.x == room.area.xMax - 1 || position.y == room.area.yMin || position.y == room.area.yMax - 1)
            {
                bool pointIsIndoor = false;

                foreach (DungeonGenerator.Room neighbor in rooms.GetNeighbors(room))
                {
                    if (pointIsInRectInt(position, neighbor.area)) pointIsIndoor = true;
                }

                if (!pointIsIndoor)
                {
                    Transform obj = GameObject.Instantiate(
                            wallPrefab,
                            new Vector3(position.x + 0.5f, wallHeight / 2f, position.y + 0.5f),
                            Quaternion.identity,
                            transform
                        );

                    obj.localScale = new Vector3(1, wallHeight, 1);
                }

                yield return new WaitForSeconds(TimeBetweenSteps);
            }
        }
    }
    private bool pointIsInRectInt(Vector2 point, RectInt rectInt)
    {
        bool pointIsIn = false;

        bool xIsIn = false;
        bool yIsIn = false;

        if (point.x >= rectInt.xMin && point.x < rectInt.xMax) xIsIn = true;
        if (point.y >= rectInt.yMin && point.y < rectInt.yMax) yIsIn = true;

        if (xIsIn && yIsIn) pointIsIn = true;

        return pointIsIn;
    }
}
