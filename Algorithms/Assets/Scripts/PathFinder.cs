using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum Algorithms
{
    BFS,
    Dijkstra,
    AStar
}
public class PathFinder : MonoBehaviour
{
    public TileMapGenerator tileMapGenerator;

    private Vector3 startNode;
    private Vector3 endNode;
    
    public List<Vector3> path = new List<Vector3>();
    HashSet<Vector3> discovered = new HashSet<Vector3>();

    private Graph<Vector3> graph = new Graph<Vector3>();
    
    public Algorithms algorithm = Algorithms.BFS;
    public void InitializeDungeon(RectInt dungeonBounds)
    {
        graph.Clear();
        int[,] tileMap = tileMapGenerator.GetTileMap();

        // Convert tilemap to Graph<Vector3>
        foreach(Vector2 pos in dungeonBounds.allPositionsWithin)
        {
            if (tileMap[(int)pos.y, (int)pos.x] == 0)
            {
                graph.AddNode(new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f));

                List<Vector2Int> neighbors = new List<Vector2Int>();

                // Diagonal
                neighbors.Add(new Vector2Int((int)pos.x - 1, (int)pos.y - 1));
                neighbors.Add(new Vector2Int((int)pos.x - 1, (int)pos.y + 1));
                neighbors.Add(new Vector2Int((int)pos.x + 1, (int)pos.y - 1));
                neighbors.Add(new Vector2Int((int)pos.x + 1, (int)pos.y + 1));

                // Horizontal and Vertical
                neighbors.Add(new Vector2Int((int)pos.x, (int)pos.y - 1));
                neighbors.Add(new Vector2Int((int)pos.x, (int)pos.y + 1));
                neighbors.Add(new Vector2Int((int)pos.x - 1, (int)pos.y));
                neighbors.Add(new Vector2Int((int)pos.x + 1, (int)pos.y));

                foreach(Vector2Int position in neighbors)
                {
                    TryConnectNeighbor(
                        position.x, position.y,
                        new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f),
                        new Vector2(position.x - pos.x, position.y - pos.y),
                        dungeonBounds, tileMap
                    );
                }
            }
        }
    }
    private void TryConnectNeighbor(int nx, int ny, Vector3 currentPos, Vector2 XYDifs, RectInt dungeonBounds, int[,] tileMap)
    {
        if (nx >= dungeonBounds.xMin && nx < dungeonBounds.xMax &&
            ny >= dungeonBounds.yMin && ny < dungeonBounds.yMax)
        {
            if (tileMap[ny, nx] == 1) return;

            if (tileMap[(int)(ny - XYDifs.y), nx] == 1 || tileMap[ny, (int)(nx - XYDifs.x)  ] == 1) return;

            Vector3 neighborPos = new Vector3(nx + 0.5f, 0, ny + 0.5f);
            graph.AddNeighbor(currentPos, neighborPos);
        }
    }
    private Vector3 GetClosestNodeToPosition(Vector3 position)
    {
        Vector3 closestNode = Vector3.zero;

        List<Vector3> nodes = graph.KeysToList();
        nodes = nodes.OrderByDescending(d => Vector3.Distance(d, position)).ToList();
        if (nodes.Count > 0) closestNode = nodes[nodes.Count - 1];

        return closestNode;
    }
    public List<Vector3> CalculatePath(Vector3 from, Vector3 to)
    {
        Vector3 playerPosition = from;
        
        startNode = GetClosestNodeToPosition(playerPosition);
        endNode = GetClosestNodeToPosition(to);

        List<Vector3> shortestPath = new List<Vector3>();
        
        switch (algorithm)
        {
            case Algorithms.BFS:
                shortestPath = BFS(startNode, endNode);
                break;
            case Algorithms.Dijkstra:
                shortestPath =  Dijkstra(startNode, endNode);
                break;
            case Algorithms.AStar:
                shortestPath = AStar(startNode, endNode);
                break;
        }
        
        path = shortestPath; //Used for drawing the path
        
        return shortestPath;
    }
    List<Vector3> BFS(Vector3 start, Vector3 end) 
    {
        Dictionary<Vector3, Vector3> nodeParents = new Dictionary<Vector3, Vector3>();
        discovered.Clear();

        Queue<Vector3> queue = new Queue<Vector3>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector3 node = queue.Dequeue();
            discovered.Add(node);

            if (node == end) return ReconstructPath(nodeParents, start, end);

            foreach (Vector3 neighbor in graph.GetNeighbors(node))
            {
                if (discovered.Contains(neighbor) || nodeParents.ContainsKey(neighbor)) continue;

                queue.Enqueue(neighbor);
                nodeParents.Add(neighbor, node);
            }
        }

        return new List<Vector3>();
    }
    public List<Vector3> Dijkstra(Vector3 start, Vector3 end)
    {
        Dictionary<Vector3, Vector3> nodeParents = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, float> nodeCosts = new Dictionary<Vector3, float>();
        discovered.Clear();

        nodeCosts.Add(start, 0);

        List<(Vector3 node, float priority)> priorityQueue = new List<(Vector3 node, float priority)>();
        priorityQueue.Add((start, 0));

        while (priorityQueue.Count > 0)
        {
            Vector3 node = priorityQueue[priorityQueue.Count - 1].node;
            discovered.Add(node);
            priorityQueue.RemoveAt(priorityQueue.Count - 1);

            if (node == end) return ReconstructPath(nodeParents, start, end);

            foreach (Vector3 neighbor in graph.GetNeighbors(node))
            {
                if (nodeParents.ContainsKey(neighbor))
                {
                    float currentCost = nodeCosts[neighbor];
                    float newPossibleCost = nodeCosts[node] + Cost(node, neighbor);

                    if (newPossibleCost < currentCost)
                    {
                        nodeCosts[neighbor] = newPossibleCost;
                        nodeParents[neighbor] = node;
                    }
                }

                else
                {
                    if (discovered.Contains(neighbor)) continue;

                    nodeParents.Add(neighbor, node);
                    nodeCosts.Add(neighbor, nodeCosts[node] + Cost(node, neighbor));
                    priorityQueue.Add((neighbor, nodeCosts[neighbor]));
                }
            }

            priorityQueue = priorityQueue.OrderByDescending(p => p.priority).ToList();
        }

        return new List<Vector3>();
    }
    List<Vector3> AStar(Vector3 start, Vector3 end)
    {
        Dictionary<Vector3, Vector3> nodeParents = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, float> nodeCosts = new Dictionary<Vector3, float>();

        discovered.Clear();

        nodeCosts.Add(start, 0);

        List<(Vector3 node, float priority)> priorityQueue = new List<(Vector3 node, float priority)>();
        priorityQueue.Add((start, 0));

        while (priorityQueue.Count > 0)
        {
            Vector3 node = priorityQueue[priorityQueue.Count - 1].node;
            discovered.Add(node);
            priorityQueue.RemoveAt(priorityQueue.Count - 1);

            if (node == end)
            {
                return ReconstructPath(nodeParents, start, end);
            }

            foreach (Vector3 neighbor in graph.GetNeighbors(node))
            {
                if (nodeParents.ContainsKey(neighbor))
                {
                    float currentCost = nodeCosts[neighbor];
                    float newPossibleCost = nodeCosts[node] + Cost(node, neighbor);

                    if (newPossibleCost < currentCost)
                    {
                        nodeCosts[neighbor] = newPossibleCost;
                        nodeParents[neighbor] = node;
                    }
                }

                else
                {
                    if (discovered.Contains(neighbor)) continue;

                    nodeParents.Add(neighbor, node);
                    nodeCosts.Add(neighbor, nodeCosts[node] + Cost(node, neighbor));
                    priorityQueue.Add((neighbor, nodeCosts[neighbor] + Heuristic(neighbor, end)));
                }
            }

            priorityQueue = priorityQueue.OrderByDescending(p => p.priority).ToList();
        }

        return new List<Vector3>();
    }
    public float Cost(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(from, to);
    }
    public float Heuristic(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(from, to);
    }
    List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> parentMap, Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 currentNode = end;

        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = parentMap[currentNode];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }
    /*
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startNode, .3f);
    
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endNode, .3f);
    
        if (discovered != null) {
            foreach (var node in discovered)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(node, .3f);
            }
        }

        if (path != null) {
            foreach (var node in path)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(node, .3f);
            }
        }
    }
    private void Update()
    {
        foreach (var node in graph.KeysToList())
        {
            DebugExtension.DebugWireSphere(node, Color.cyan, .2f);
            foreach (var neighbor in graph.GetNeighbors(node))
            {
                Debug.DrawLine(node, neighbor, Color.cyan);
            }
        }
    }
    */
}