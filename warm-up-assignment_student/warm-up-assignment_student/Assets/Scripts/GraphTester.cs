using UnityEngine;
public class GraphTester : MonoBehaviour
{
    void Start()
    {
        Graph<string> graph = new Graph<string>();
        graph.AddRoom("A");
        graph.AddRoom("B");
        graph.AddRoom("C");
        graph.AddRoom("D");
        graph.AddNeighbor("A", "B");
        graph.AddNeighbor("A", "C");
        graph.AddNeighbor("B", "D");
        graph.AddNeighbor("C", "D");
        Debug.Log("Graph Structure:");
        graph.PrintGraph();
    }
}