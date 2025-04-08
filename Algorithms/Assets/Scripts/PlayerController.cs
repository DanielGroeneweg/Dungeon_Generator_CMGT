using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
public class PlayerController : MonoBehaviour
{
    public Vector3 clickPosition;
    public UnityEvent<Vector3> OnClick;

    [SerializeField] private NavMeshAgent navMeshAgent;
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
            }
        }

        // Add visual debugging here
        Debug.DrawLine(Camera.main.transform.position, clickPosition);
    }

    public void GoToDestination(Vector3 destination)
    {
        navMeshAgent.SetDestination(destination);
    }
}
