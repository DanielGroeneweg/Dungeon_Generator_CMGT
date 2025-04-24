using UnityEngine;
public class MouseClickController : MonoBehaviour
{
    public Vector3 clickPosition;
    private FollowPathController player;
    
    // Update is called once per frame
    void Update()
    {
        // Get the mouse click position in world space
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay( Input.mousePosition );
            if (Physics.Raycast( mouseRay, out RaycastHit hitInfo ))
            {
                Vector3 clickWorldPosition = hitInfo.point;
                clickPosition = clickWorldPosition;

                if (player != null) player.GoToDestination(clickWorldPosition);
            }
        }
        
        DebugExtension.DebugWireSphere(clickPosition, Color.yellow, .1f);
        Debug.DrawLine(Camera.main.transform.position, clickPosition, Color.yellow);
    }
    public void InitializePlayer(FollowPathController _Player)
    {
        player = _Player;
    }
}