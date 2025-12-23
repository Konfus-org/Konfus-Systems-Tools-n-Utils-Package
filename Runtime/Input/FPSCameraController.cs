using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class FPSCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        [Tooltip("Usually the player body.")]
        private Transform? yawTarget;
        [SerializeField]
        [Tooltip("Usually the camera root.")]
        private Transform? pitchTarget;
        [Header("Look Settings")]
        [SerializeField]
        private Vector2 sensitivity = Vector2.zero;
        [SerializeField]
        private Vector2 smoothAmount = Vector2.zero;
        [SerializeField]
        [Range(-90f, 90f)]
        private Vector2 lookAngleMinMax = Vector2.zero;

        private float _desiredPitch;
        private float _desiredYaw;
        private Vector2 _lookInput;
        private float _pitch;
        private float _yaw;

        private void Awake()
        {
            _yaw = transform.eulerAngles.y;
            _desiredYaw = _yaw;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }

        private void LateUpdate()
        {
            CalculateRotation();
            SmoothRotation();
            ApplyRotation();
        }

        public void OnLookInput(Vector2 input)
        {
            _lookInput = input;
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
            if (!yawTarget) return;
            yawTarget.eulerAngles = new Vector3(0f, _yaw, 0f);
            if (!pitchTarget) return;
            pitchTarget.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }
    }
}