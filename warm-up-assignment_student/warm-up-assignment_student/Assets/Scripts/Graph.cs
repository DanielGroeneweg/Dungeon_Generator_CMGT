using System.Collections.Generic;
using UnityEngine;

public class Graph<T>
{
    public Dictionary<T, List<T>> adjacencyList;
    public Graph() { adjacencyList = new Dictionary<T, List<T>>(); }
    public void AddRoom(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            adjacencyList[node] = new List<T>();
        }
    }
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
    public List<T> GetNeighbors(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            Debug.Log("Room does not exist in the graph.");
        }
        return adjacencyList[node];
    }
    public void RemoveFromNeighbors(T nodeToRemove)
    {
        foreach (T key in adjacencyList.Keys)
        {
            if (adjacencyList[key].Contains(nodeToRemove)) adjacencyList[key].Remove(nodeToRemove);
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

            Debug.Log(key + " has neighbors: " + neighbors);
        }
    }
    public List<T> KeysToList()
    {
        List<T> list = new List<T>();

        foreach (T key in adjacencyList.Keys) list.Add(key);

        return list;
    }
    public void DFS(T v)
    {
        HashSet<T> discovered = new HashSet<T>();
        Stack<T> S = new Stack<T>();
        S.Push(v);
        while (S.Count > 0)
        {
            v = S.Pop();
            if (!discovered.Contains(v))
            {
                discovered.Add(v);
                foreach(T W in GetNeighbors(v))
                {
                    S.Push(W);
                }
            }
        }
    }
}