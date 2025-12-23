using UnityEngine;

namespace Konfus.Input
{
    // TODO: update this to work with cinemachine
    public class RtsCameraController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private float minFov = 15f;
        [SerializeField]
        private float maxFov = 90f;
        [SerializeField]
        [Tooltip("Min angle camera can rotate to in degrees relative to origin")]
        private float minAngle = -80f;
        [SerializeField]
        private float maxAngle = 0.1f;
        [SerializeField]
        private float zoomSensitivity = 10f;
        [SerializeField]
        private float sensitivityX = 2f;
        [SerializeField]
        private float sensitivityY = 2f;
        [SerializeField]
        private float moveSpeed = 1f;

        [Header("Dependencies")]
        [SerializeField]
        private Camera? mainCamera;

        private Vector2 _moveInput;
        private float _pitch;
        private Vector2 _rotateInput;
        private float _yaw;
        private float _zoomInput;

        public Camera? Camera => mainCamera;

        private void Start()
        {
            _pitch = transform.eulerAngles.x;
            _yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (_rotateInput != Vector2.zero) Rotate();
            if (_moveInput != Vector2.zero) Move();
            UpdateZoom();
        }

        public void OnRotateInput(Vector2 rotateInput)
        {
            _rotateInput = rotateInput;
        }

        public void OnMoveInput(Vector2 moveInput)
        {
            _moveInput = moveInput;
        }

        public void OnZoomInput(float zoomInput)
        {
            _zoomInput = zoomInput;
        }

        private void UpdateZoom()
        {
            if (!mainCamera)
            {
                Debug.LogWarning("Main camera not set");
                return;
            }

            float fov = mainCamera.fieldOfView;
            fov += _zoomInput * zoomSensitivity;
            fov = Mathf.Clamp(fov, minFov, maxFov);
            mainCamera.fieldOfView = fov;
        }

        private void Rotate()
        {
            _yaw += sensitivityX * _rotateInput.x;
            _pitch += sensitivityY * _rotateInput.y;
            _pitch = Mathf.Clamp(_pitch, minAngle, maxAngle);
            transform.eulerAngles = new Vector3(_pitch, _yaw, 0.0f);
        }

        private void Move()
        {
            if (!mainCamera)
            {
                Debug.LogWarning("Main camera not set");
                return;
            }

            Transform camTransform = mainCamera.transform;
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = camForward * _moveInput.y + camRight * _moveInput.x;
            Vector3 moveTo = new(moveDir.x, 0, moveDir.z);

            transform.Translate(moveTo * moveSpeed, Space.World);
        }
    }
}