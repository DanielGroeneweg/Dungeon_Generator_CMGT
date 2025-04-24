using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Graph<T>
{
    public Dictionary<T, List<T>> adjacencyList;
    public Graph() { adjacencyList = new Dictionary<T, List<T>>(); }
    /// <summary>
    /// Adds the node to the list without any connections made
    /// </summary>
    /// <param name="node"></param>
    public void AddNode(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            adjacencyList[node] = new List<T>();
        }
        else
        {
            Debug.LogWarning("Node is already in the graph.");
        }
    }
    public void RemoveNode(T node)
    {
        RemoveFromNeighbors(node);
        adjacencyList.Remove(node);
    }
    /// <summary>
    /// Creates a connection between two nodes, making them neighbors
    /// </summary>
    /// <param name="fromNode"></param>
    /// <param name="toNode"></param>
    public void AddNeighbor(T fromNode, T toNode)
    {
        if (!adjacencyList.ContainsKey(fromNode))
        {
            Debug.Log("from node does not exist in the graph.");
            return;
        }

        if (!adjacencyList[fromNode].Contains(toNode)) adjacencyList[fromNode].Add(toNode);

        if (!adjacencyList.ContainsKey(toNode))
        {
            return;
        }

        if (!adjacencyList[toNode].Contains(fromNode)) adjacencyList[toNode].Add(fromNode);
    }
    /// <summary>
    /// Returns a list of all neighboring nodes
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public List<T> GetNeighbors(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            Debug.Log("Node does not exist in the graph.");
        }
        return adjacencyList[node];
    }
    /// <summary>
    /// Removes a connection between two given nodes
    /// </summary>
    /// <param name="fromNode"></param>
    /// <param name="toNode"></param>
    public void RemoveNeighbor(T fromNode, T toNode)
    {
        if (!adjacencyList.ContainsKey(fromNode))
        {
            Debug.Log("from node does not exist in the graph.");
            return;
        }

        if (!adjacencyList.ContainsKey(toNode))
        {
            Debug.Log("to node does not exist in the graph.");
            return;
        }

        if (adjacencyList[fromNode].Contains(toNode)) adjacencyList[fromNode].Remove(toNode);

        if (adjacencyList[toNode].Contains(fromNode)) adjacencyList[toNode].Remove(fromNode);
    }
    /// <summary>
    /// Removes a node as neighbor from all other nodes
    /// </summary>
    /// <param name="nodeToRemove"></param>
    public void RemoveFromNeighbors(T nodeToRemove)
    {
        if (adjacencyList.Keys.Count == 0) return;
        foreach (T key in adjacencyList.Keys)
        {
            if (adjacencyList[key].Contains(nodeToRemove)) adjacencyList[key].Remove(nodeToRemove);
        }
    }
    /// <summary>
    /// Removes all neighbors in adjacencylist
    /// </summary>
    public void ClearNeighbors()
    {
        foreach (T key in adjacencyList.Keys) adjacencyList[key].Clear();
    }
    public void Clear()
    {
        List<T> list = KeysToList();
        for (int i = list.Count - 1; i >= 0; i--)
        {
            list.RemoveAt(i);
        }
    }
    public void PrintGraph()
    {
        foreach (T key in adjacencyList.Keys)
        {
            string neighbors = "";
            foreach (T neighbor in adjacencyList[key])
            {
                neighbors += ", " + neighbor;
            }

            Debug.Log(key + " has " + adjacencyList[key].Count + " neighbors: " + neighbors);
        }
    }
    /// <summary>
    /// Returns a list of all keys in the dictionary
    /// </summary>
    /// <returns></returns>
    public List<T> KeysToList()
    {
        return new List<T>(adjacencyList.Keys);
    }
    /// <summary>
    /// Uses Depth First Searching to create a spanning tree without loops. Requires connections to already exist
    /// </summary>
    /// <param name="node"></param>
    public void DFS(T node)
    {
        HashSet<T> discovered = new HashSet<T>();
        Stack<T> stack = new Stack<T>();

        stack.Push(node);

        Dictionary<T, T> nodeCameFrom = new Dictionary<T, T>();

        while (stack.Count > 0)
        {
            node = stack.Pop();

            if (!discovered.Contains(node))
            {
                discovered.Add(node);
                foreach (T neighbor in GetNeighbors(node))
                {
                    stack.Push(neighbor);
                    if (!discovered.Contains(neighbor)) nodeCameFrom[neighbor] = node;
                }
            }
        }

        ClearNeighbors();

        foreach (T key in nodeCameFrom.Keys)
        {
            AddNeighbor(key, nodeCameFrom[key]);
        }
    }
    /// <summary>
    /// Uses Breadth First Searching to create a spanning tree without loops. Requires connections to already exist
    /// </summary>
    /// <param name="node"></param>
    /// <param name="random"></param>
    /// <param name="seed"></param>
    public void BFS(T node)
    {
        HashSet<T> discovered = new HashSet<T>();
        Queue<T> queue = new Queue<T>();
        Dictionary<T, T> nodeCameFrom = new Dictionary<T, T>();

        queue.Enqueue(node);
        discovered.Add(node);

        while (queue.Count > 0)
        {
            node = queue.Dequeue();

            foreach (T neighbor in GetNeighbors(node))
            {
                if (!discovered.Contains(neighbor))
                {
                    discovered.Add(neighbor);
                    queue.Enqueue(neighbor);
                    nodeCameFrom[neighbor] = node;
                }
            }
        }

        ClearNeighbors();

        foreach (T key in nodeCameFrom.Keys)
        {
            AddNeighbor(key, nodeCameFrom[key]);
        }
    }
}