using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Konfus.Systems.ThreeDCursor
{
    public class ThreeDCursor : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Tooltip("First in list will be starting state!")] 
        private ThreeDCursorState[] states;

        [Header("Dependencies")] 
        [SerializeField] 
        private Camera mainCamera;
        [SerializeField] 
        private Camera cursorCamera;
    
        private ThreeDCursorState _activeState;
        private Dictionary<string, GameObject> _stateInstances;
        private Vector2 _mouseInput;

        /// <summary>
        /// Call when there is mouse input. Will update the position of the 3d cursor to match the mouse position.
        /// </summary>
        /// <param name="mousePos"> The mouse input. </param>
        public void OnMouseInput(Vector2 mouseInput)
        {
            _mouseInput = mouseInput;
        }

        /// <summary>
        /// Sets cursor state from a states name.
        /// </summary>
        /// <param name="name"> The name of the state to set the cursor to. </param>
        public void SetState(string name)
        {
            // Disable last state and set active state
            _stateInstances[_activeState.name].SetActive(false);
            _activeState = states.First(s => s.name == name);
        
            // Update transform and set new state to active
            UpdateTransform();
            _stateInstances[name].SetActive(true);
        }

        private void Start()
        {
            // Disable 2d system cursor so we can replace it with the 3d cursor!
            Cursor.visible = false;
        
            // Cursor camera must match main camera!
            cursorCamera.fieldOfView = mainCamera.fieldOfView;
            cursorCamera.farClipPlane = mainCamera.farClipPlane;
            cursorCamera.nearClipPlane = mainCamera.nearClipPlane;
            cursorCamera.orthographic = mainCamera.orthographic;
            cursorCamera.orthographicSize = mainCamera.orthographicSize;
        
            // Create and cache the cursor states...
            _stateInstances = new Dictionary<string, GameObject>();
            foreach (ThreeDCursorState state in states)
            {
                GameObject stateInstance = Instantiate(state.Visual, transform);
                stateInstance.name = state.name;
                stateInstance.SetActive(false);
                _stateInstances.Add(state.name, stateInstance);
            }
        
            // Set starting state
            ThreeDCursorState startingState = states.First();
            _stateInstances[startingState.name].SetActive(true);
            _activeState = startingState;
        }

        private void Update()
        {
            UpdateTransform();
        }

        private void UpdateTransform()
        {
            // Update transform from input, camera position, and settings...
            Transform mainCameraTransform = mainCamera.transform;
            var offset = new Vector2(_activeState.Offset.x * transform.localScale.x, _activeState.Offset.y * transform.localScale.y);
            var screenPoint = new Vector3(_mouseInput.x, _mouseInput.y, -mainCameraTransform.position.z);
            transform.position = mainCamera.ScreenToWorldPoint(screenPoint) - new Vector3(offset.x, 0, offset.y);
            transform.rotation = mainCameraTransform.rotation;
            transform.localScale = Vector3.one * _activeState.Size;
        }
    }
}
