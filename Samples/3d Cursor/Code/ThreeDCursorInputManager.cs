using Konfus.Systems.ThreeDCursor;
using UnityEngine;

namespace Konfus_Systems_Tools_n__Utils_Package.Samples._3d_Cursor.Code
{
    /// <summary>
    /// Example of how to use the 3d cursor, I don't recommend doing this exactly I would at least
    /// utilize the new input system instead of the old one like I'm doing here...
    /// </summary>
    public class ThreeDCursorInputManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private LayerMask layersToHover;
    
        [Header("Dependencies")]
        [SerializeField]
        private ThreeDCursor threeDCursor;
        [SerializeField] 
        private Camera mainCamera;

        private void Update()
        {
            Vector3 mousePos = Input.mousePosition;
            
            if (Input.GetMouseButton(0)) OnClick();
            else if (Input.GetMouseButton(1)) OnZoom();
            else if (Physics.Raycast(ray: mainCamera.ScreenPointToRay(mousePos), layerMask: layersToHover, maxDistance: 10f))
            {
                OnHover();
            }
            else
            {
                threeDCursor.SetState("Idle");
            }

            threeDCursor.OnMouseInput(mousePos);
        }

        private void OnClick()
        {
            threeDCursor.SetState("Click");
        }

        private void OnHover()
        {
            threeDCursor.SetState("Hover");
        }

        private void OnZoom()
        {
            threeDCursor.SetState("Magnify");
        }
    }
}
