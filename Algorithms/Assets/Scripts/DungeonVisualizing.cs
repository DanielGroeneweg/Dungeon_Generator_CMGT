using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.AI.Navigation;
public class DungeonVisualizing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshSurface surface;

    [Header("Prefabs")]
    [SerializeField] private Transform floorPrefab;
    [SerializeField] private Transform wallPrefab;
    [SerializeField] private PlayerController playerPrefab;

    [Header("Stats")]
    [SerializeField] private float wallHeight = 4;

    [Header("Hierarchy")]
    [SerializeField] private Transform roomsParentObject;

    private Graph<DungeonGenerator.Room> rooms;
    private Graph<DungeonGenerator.Room> doors;

    private float TimeBetweenSteps = 0;

    private HashSet<Vector2> discoveredPositions;

    private PlayerController player;

    private int finishedWallsCount = 0;
    private int finishedFloorsCount = 0;
    private void Start()
    {
        discoveredPositions= new HashSet<Vector2>();
    }
    public void MakeDungeonPhysical(Graph<DungeonGenerator.Room> roomGraph, Graph<DungeonGenerator.Room> doorGraph, float time)
    {
        TimeBetweenSteps = time;

        rooms = roomGraph;
        doors = doorGraph;

        StartCoroutine(Generate());
    }
    public void ClearDungeon()
    {
        StopAllCoroutines();
        surface.RemoveData();
        discoveredPositions.Clear();

        finishedWallsCount = 0;
        finishedFloorsCount = 0;

        // Remove all assets
        foreach (Transform child in roomsParentObject)
        {
            Destroy(child.gameObject);
        }
    }
    private IEnumerator Generate()
    {
        // Save door positions
        foreach (DungeonGenerator.Room door in doors.KeysToList())
        {
            foreach(Vector2 position in door.area.allPositionsWithin)
            {
                discoveredPositions.Add(position);
            }
        }

        // Room generating
        foreach(DungeonGenerator.Room room in rooms.KeysToList())
        {
            // Create parent object for this room
            GameObject parentObject = new GameObject("Room_" + room.area.xMin + "_" + room.area.yMin + "_" + room.area.xMax + "_" + room.area.yMax);
            parentObject.transform.parent = roomsParentObject;

            // Generate floor for this room
            GenerateFloor(room, parentObject);

            // Generate walls for this room
            StartCoroutine(MakeWalls(room, parentObject));

            // Coroutine
            yield return new WaitForSeconds(TimeBetweenSteps);
        }

        yield return new WaitUntil(() => (finishedWallsCount == rooms.KeysToList().Count && finishedFloorsCount == rooms.KeysToList().Count));

        // Bake NavMesh
        surface.BuildNavMesh();

        // Spawn player in the first room of the list
        DungeonGenerator.Room firstRoom = rooms.KeysToList()[0];
        Vector2 pos = firstRoom.area.center;
        player = GameObject.Instantiate(
            playerPrefab,
            new Vector3(pos.x, 0.1f, pos.y),
            Quaternion.identity,
            roomsParentObject.transform
        );
        player.gameObject.name = "Player";

        Debug.Log(rooms.adjacencyList.Count);
        Debug.Log(doors.adjacencyList.Count);

        player.InitializeDungeonData(rooms, doors);
    }
    private void GenerateFloor(DungeonGenerator.Room room, GameObject parentObject)
    {
        Transform obj = GameObject.Instantiate(
            floorPrefab,
            new Vector3(room.area.center.x, 0, room.area.center.y),
            Quaternion.identity,
            parentObject.transform
            );

        obj.name = "Floor";
        obj.localScale = new Vector3(room.area.width, room.area.height, 1);
        obj.localEulerAngles = new Vector3(90, 0, 0);
        finishedFloorsCount++;
    }
    private IEnumerator MakeWalls(DungeonGenerator.Room room, GameObject parentObject)
    {
        foreach (Vector2 position in room.area.allPositionsWithin)
        {
            if (position.x == room.area.xMin || position.x == room.area.xMax - 1 || position.y == room.area.yMin || position.y == room.area.yMax - 1)
            {
                if (discoveredPositions.Contains(position)) continue;

                Transform obj = GameObject.Instantiate(
                        wallPrefab,
                        new Vector3(position.x + 0.5f, wallHeight / 2f, position.y + 0.5f),
                        Quaternion.identity,
                        parentObject.transform
                    );

                obj.localScale = new Vector3(1, wallHeight, 1);
                obj.name = "Wall_" + position.x + "_" + position.y;

                discoveredPositions.Add(position);

                yield return new WaitForSeconds(TimeBetweenSteps);
            }
        }
        finishedWallsCount++;
    }
}
