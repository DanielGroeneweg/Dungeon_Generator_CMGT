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
            Debug.Log("from room does not exist in the graph.");
            return;
        }
        adjacencyList[fromNode].Add(toNode);

        if (adjacencyList.ContainsKey(toNode)) adjacencyList[toNode].Add(fromNode);
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
            Debug.Log("Room does not exist in the graph.");
        }
        return adjacencyList[node];
    }
    public int GetNeighborCount(T node)
    {
        return adjacencyList[node].Count;
    }
    /// <summary>
    /// Removes a node as neighbor from all other nodes
    /// </summary>
    /// <param name="nodeToRemove"></param>
    public void RemoveFromNeighbors(T nodeToRemove)
    {
        foreach (T key in adjacencyList.Keys)
        {
            if (adjacencyList[key].Contains(nodeToRemove)) adjacencyList[key].Remove(nodeToRemove);
        }
    }
    public void ClearNeighbors()
    {
        foreach (T key in adjacencyList.Keys) adjacencyList[key].Clear();
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

            Debug.Log(key + " has " + GetNeighborCount(key) + " neighbors: " + neighbors);
        }
    }
    /// <summary>
    /// Returns a list of all keys in the dictionary
    /// </summary>
    /// <returns></returns>
    public List<T> KeysToList()
    {
        List<T> list = new List<T>();

        foreach (T key in adjacencyList.Keys) list.Add(key);

        return list;
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

                    if (!nodeCameFrom.ContainsKey(neighbor)) nodeCameFrom.Add(neighbor, node);
                    else nodeCameFrom[neighbor] = node;
                }
            }
        }

        ClearNeighbors();

        foreach (T key in nodeCameFrom.Keys)
        {
            if (!adjacencyList[key].Contains(nodeCameFrom[key])) AddNeighbor(key, nodeCameFrom[key]);
        }
    }
}