using UnityEngine;
public class GraphTester : MonoBehaviour
{
    void Start()
    {
        Graph<string> graph = new Graph<string>();
        graph.AddNode("A");
        graph.AddNode("B");
        graph.AddNode("C");
        graph.AddNode("D");
        graph.AddNeighbor("A", "B");
        graph.AddNeighbor("A", "C");
        graph.AddNeighbor("B", "D");
        graph.AddNeighbor("C", "D");
        Debug.Log("Graph Structure:");
        graph.PrintGraph();
    }
}