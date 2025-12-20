using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    [RequireComponent(typeof(Camera))]
    public class FPSCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerInput input;
        [SerializeField, Tooltip("Usually the player body.")] 
        private Transform yawTarget;
        [SerializeField, Tooltip("Usually the camera root.")] 
        private Transform pitchTarget;
        
        [Header("Look Settings")] 
        [SerializeField]
        private Vector2 sensitivity = Vector2.zero;
        [SerializeField] 
        private Vector2 smoothAmount = Vector2.zero;
        [SerializeField] 
        [Range(-90f, 90f)] 
        private Vector2 lookAngleMinMax = Vector2.zero;
        
        private float _yaw;
        private float _pitch;
        private float _desiredYaw;
        private float _desiredPitch;

        public Vector2 _lookInput;
        
        public void OnLookInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                _lookInput = ctx.ReadValue<Vector2>();
            else if (ctx.canceled)
                _lookInput = Vector2.zero;
        }

        // TODO: move to a reticle class
        public void ToggleCursorLock()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }

        private void Awake()
        {
            _yaw = transform.eulerAngles.y;
            _desiredYaw = _yaw;
            ToggleCursorLock();
        }

        private void LateUpdate()
        {
            CalculateRotation();
            SmoothRotation();
            ApplyRotation();
        }

        private void CalculateRotation()
        {
            _desiredYaw += _lookInput.x * sensitivity.x * Time.deltaTime;
            _desiredPitch -= _lookInput.y * sensitivity.y * Time.deltaTime;
            _desiredPitch = Mathf.Clamp(_desiredPitch, lookAngleMinMax.x, lookAngleMinMax.y);
        }

        private void SmoothRotation()
        {
            _yaw = Mathf.Lerp(_yaw, _desiredYaw, smoothAmount.x * Time.deltaTime);
            _pitch = Mathf.Lerp(_pitch, _desiredPitch, smoothAmount.y * Time.deltaTime);
        }

        private void ApplyRotation()
        {
            yawTarget.eulerAngles = new Vector3(0f, _yaw, 0f);
            pitchTarget.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }
    }
}