using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public enum PathFinding { UnityAgent, Selfmade}
public class FollowPathController : MonoBehaviour
{
    public PathFinding pathFinding = PathFinding.UnityAgent;
    [SerializeField] 
    private PathFinder pathFinder;
    
    [SerializeField]
    private float speed = 5f;

    private bool isMoving = false;
    public void FindPathFinder(PathFinder _PathFinder)
    {
        pathFinder = _PathFinder;
    }
    public void GoToDestination(Vector3 destination)
    {
        if (isMoving) return;
        switch(pathFinding)
        {
            case PathFinding.UnityAgent:
                GameObject.Find("Player").GetComponent<NavMeshAgent>().enabled = true;
                GameObject.Find("Player").GetComponent<NavMeshAgent>().SetDestination(destination);
                break;
            case PathFinding.Selfmade:
                GameObject.Find("Player").GetComponent<NavMeshAgent>().enabled = false;
                StartCoroutine(FollowPathCoroutine(pathFinder.CalculatePath(transform.position, destination)));
                break;
        }
    }
    IEnumerator FollowPathCoroutine(List<Vector3> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.Log("No path found");
            yield break;
        }
        isMoving = true;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 target = path[i];
            target.y = transform.position.y;
            // Move towards the target position
            while (Vector3.Distance(transform.position, target) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
                yield return null;
            }
            
            Debug.Log($"Reached target: {target}");
        }
        isMoving = false;
    }
}