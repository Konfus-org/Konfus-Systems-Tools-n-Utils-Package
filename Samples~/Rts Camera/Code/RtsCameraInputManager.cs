using Konfus.Systems.Cameras;
using UnityEngine;

namespace Konfus_Systems_Tools_n_Utils_Package.Samples.Rts_Camera.Code
{
    /// <summary>
    /// Sample usage of rts camera, I don't recommend using this as is!
    /// I would at least update to use new input system!
    /// </summary>
    public class RtsCameraInputManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] 
        private RtsCamera rtsCamera;
        
        private void Update()
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            bool rightClick = Input.GetButton("Right Click");
            bool leftClick = Input.GetButton("Left Click");
            float zoom = Input.GetAxis("Mouse ScrollWheel");
            
            if (rightClick || leftClick) rtsCamera.OnRotateInput(new Vector2(mouseX, mouseY).normalized);
            else rtsCamera.OnRotateInput(Vector2.zero);
            rtsCamera.OnMoveInput(new Vector2(moveX, moveY).normalized);
            rtsCamera.OnZoomInput(zoom);
        }
    }

}