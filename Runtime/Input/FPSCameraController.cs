using Unity.Cinemachine;
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
        [Range(0f, 100f)]
        private float xSensitivity = 50.0f;
        [SerializeField]
        [Range(0f, 100f)]
        private float ySensitivity = 50.0f;
        [SerializeField]
        [Range(1f, 100f)]
        private float xSmoothing = 30f;
        [SerializeField]
        [Range(1f, 100f)]
        private float ySmoothing = 30f;
        [SerializeField]
        [MinMaxRangeSlider(-90, 90)]
        private Vector2 lookAngleMinMax = new(-80, 85);

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

        public void Look(Vector2 input)
        {
            _lookInput = input;
        }

        private void CalculateRotation()
        {
            _desiredYaw += _lookInput.x * xSensitivity * Time.deltaTime;
            _desiredPitch -= _lookInput.y * ySensitivity * Time.deltaTime;
            _desiredPitch = Mathf.Clamp(_desiredPitch, lookAngleMinMax.x, lookAngleMinMax.y);
        }

        private void SmoothRotation()
        {
            _yaw = Mathf.Lerp(_yaw, _desiredYaw, 100 - xSmoothing * Time.deltaTime);
            _pitch = Mathf.Lerp(_pitch, _desiredPitch, 100 - ySmoothing * Time.deltaTime);
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