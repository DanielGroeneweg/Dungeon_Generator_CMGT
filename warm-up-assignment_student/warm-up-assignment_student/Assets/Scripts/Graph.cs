using System.Collections.Generic;
using UnityEngine;

public class Graph<T>
{
    public Dictionary<T, List<T>> adjacencyList;
    public Graph() { adjacencyList = new Dictionary<T, List<T>>(); }
    public void AddRoom(T room)
    {
        if (!adjacencyList.ContainsKey(room))
        {
            adjacencyList[room] = new List<T>();
        }
    }
    public void AddNeighbor(T fromRoom, T toRoom)
    {
        if (!adjacencyList.ContainsKey(fromRoom))
        {
            Debug.Log("from room does not exist in the graph.");
            return;
        }
        adjacencyList[fromRoom].Add(toRoom);
        
        if (adjacencyList.ContainsKey(toRoom)) adjacencyList[toRoom].Add(fromRoom);
    }
    public List<T> GetNeighbors(T room)
    {
        if (!adjacencyList.ContainsKey(room))
        {
            Debug.Log("Room does not exist in the graph.");
        }
        return adjacencyList[room];
    }
    public void PrintGraph()
    {
        foreach(var room in adjacencyList.Keys)
        {
            string neighbors = "";
            foreach(var neighbor in adjacencyList[room])
            {
                neighbors += neighbor + ",";
            }

            Debug.Log(room + " has neighbors: " + neighbors);
        }
    }
    public void Test(T room)
    {
        var something = adjacencyList[room];
    }
}