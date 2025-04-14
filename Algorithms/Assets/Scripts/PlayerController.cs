using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEditor.Experimental.GraphView;
public class PlayerController : MonoBehaviour
{
    public Vector3 clickPosition;
    public UnityEvent<Vector3> OnClick;

    private List<DungeonGenerator.Room> roomsInPath;
    private Graph<DungeonGenerator.Room> rooms;

    [SerializeField] private NavMeshAgent navMeshAgent;
    private void Awake()
    {
        roomsInPath = new List<DungeonGenerator.Room>();
        rooms = new Graph<DungeonGenerator.Room>();
    }
    void Update()
    {
        // Get the mouse click position in world space 
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo))
            {
                Vector3 clickWorldPosition = hitInfo.point;
                Debug.Log(clickWorldPosition);

                // Store the click position here
                clickPosition = clickWorldPosition;

                // Trigger an unity event to notify other scripts about the click here
                OnClick.Invoke(clickPosition);

                // Create path
                PathFinding();
            }
        }
    }
    public void InitializeDungeonData(Graph<DungeonGenerator.Room> _rooms, Graph<DungeonGenerator.Room> _doors)
    {
        Debug.Log("-----------------------------------------------------------");
        // Merge room and door graphs into one
        foreach(DungeonGenerator.Room room in _rooms.KeysToList())
        {
            rooms.AddNode(room);
        }

        foreach(DungeonGenerator.Room door in _doors.KeysToList())
        {
            rooms.AddNode(door);
            foreach (DungeonGenerator.Room neighbor in _doors.GetNeighbors(door))
            {
                rooms.AddNeighbor(door, neighbor);
            }
        }
    }
    public void GoToDestination(Vector3 destination)
    {
        navMeshAgent.SetDestination(destination);
    }
    private void PathFinding()
    {
        DungeonGenerator.Room targetRoom = FindRoomWithPosition(clickPosition);

        if (!rooms.adjacencyList.ContainsKey(targetRoom))
        {
            Debug.LogWarning("Rooms Does not contain this room!");
            return;
        }

        FindPathToRoom(targetRoom);
    }
    private DungeonGenerator.Room FindRoomWithPosition(Vector3 position)
    {
        foreach (DungeonGenerator.Room room in rooms.KeysToList())
        {
            if (pointIsInRoom(room, position)) return room;
        }

        Debug.LogWarning("Position Not in Dungeon");
        return null;
    }
    private bool pointIsInRoom(DungeonGenerator.Room room, Vector3 point)
    {
        int wallbuffer = 1;
        if (room.isDoor) wallbuffer = 0;

        if (point.x >= room.area.xMin + wallbuffer &&
            point.x <= room.area.xMax - wallbuffer &&
            point.z >= room.area.yMin + wallbuffer &&
            point.z <= room.area.yMax - wallbuffer)
            return true;

        else return false;
    }
    private void FindPathToRoom(DungeonGenerator.Room targetRoom)
    {
        if (pointIsInRoom(targetRoom, transform.position))
        {
            GoToPoint();
            return;
        }

        BFSPathSearching(FindRoomWithPosition(transform.position), targetRoom);
    }
    private void BFSPathSearching(DungeonGenerator.Room fromRoom, DungeonGenerator.Room toRoom)
    {
        DungeonGenerator.Room room;

        Dictionary<DungeonGenerator.Room, DungeonGenerator.Room> roomCameFrom = new Dictionary<DungeonGenerator.Room, DungeonGenerator.Room>();

        HashSet<DungeonGenerator.Room> discovered = new HashSet<DungeonGenerator.Room>();
        Queue<DungeonGenerator.Room> queue = new Queue<DungeonGenerator.Room>();

        queue.Enqueue(fromRoom);
        discovered.Add(fromRoom);

        while (queue.Count > 0)
        {
            room = queue.Dequeue();

            foreach (DungeonGenerator.Room neighbor in rooms.GetNeighbors(room))
            {
                if (!discovered.Contains(neighbor))
                {
                    if (neighbor == toRoom)
                    {
                        roomCameFrom[neighbor] = room;
                        queue.Clear();
                    }

                    else
                    {
                        discovered.Add(neighbor);
                        queue.Enqueue(neighbor);
                        roomCameFrom[neighbor] = room;
                    }
                }
            }
        }

        CreatePathList(roomCameFrom, fromRoom, toRoom);
    }
    private void CreatePathList(Dictionary<DungeonGenerator.Room, DungeonGenerator.Room> roomOrigins, DungeonGenerator.Room fromRoom, DungeonGenerator.Room toRoom)
    {
        roomsInPath.Clear();

        roomsInPath.Insert(0, toRoom);

        DungeonGenerator.Room room = roomOrigins[toRoom];

        while (room != fromRoom)
        {
            roomsInPath.Insert(0, room);
            room = roomOrigins[room];
        }

        roomsInPath.Insert(0, room);
    }
    private void FixedUpdate()
    {
        for (int i = 0; i <= roomsInPath.Count - 2; i++)
        {
            DungeonGenerator.Room room = roomsInPath[i];
            Debug.DrawLine(new Vector3(roomsInPath[i].area.center.x, 1, roomsInPath[i].area.center.y), new Vector3(roomsInPath[i + 1].area.center.x, 1, roomsInPath[i + 1].area.center.y), Color.yellow);
        }
    }
    private void GoToPoint()
    {
        Debug.Log("Loser");
    }
}