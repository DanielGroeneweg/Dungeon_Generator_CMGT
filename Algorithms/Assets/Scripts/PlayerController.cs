using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Vector3 clickPosition;

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

            }
        }

        // Add visual debugging here
        Debug.DrawLine(Camera.main.transform.position, clickPosition);
    }
}
