using Unity.Cinemachine;
using UnityEngine;

namespace Konfus.Input
{
    [RequireComponent(typeof(CinemachineCamera))]
    [DisallowMultipleComponent]
    public class FPSCamera : MonoBehaviour
    {
        private const float IdleSpeed = 0.1f;
        private const float MaxSpeedForBob = 7f;

        [Header("References")]
        [SerializeField]
        [Tooltip("Usually the player body.")]
        private Transform? yawTarget;
        [SerializeField]
        [Tooltip("Camera pitch pivot.")]
        private Transform? pitchTarget;
        [SerializeField]
        [Tooltip("Transform used to calculate movement speed.")]
        private Transform? speedTarget;

        [Header("Look Settings")]
        [SerializeField]
        [Range(0f, 100f)]
        private float xSensitivity = 50f;
        [SerializeField]
        [Range(0f, 100f)]
        private float ySensitivity = 50f;
        [SerializeField]
        [Range(0f, 0.25f)]
        private float smoothTime = 0.06f;
        [SerializeField]
        private Vector2 lookAngleMinMax = new(-80f, 85f);

        [Header("Head Bob")]
        [SerializeField]
        private AnimationCurve amplitudeBySpeed =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField]
        private AnimationCurve frequencyBySpeed =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField]
        private float amplitudeMultiplier = 1f;
        [SerializeField]
        private float frequencyMultiplier = 1f;
        [SerializeField]
        [Tooltip("How quickly bob reacts to speed changes.")]
        private float bobResponse = 12f;
        private CinemachineCamera? _camera;

        private float _currentAmp;
        private float _currentFreq;
        private float _desiredPitch;
        private float _desiredYaw;

        private Vector3 _lastSpeedPos;
        private Vector2 _lookInput;
        private CinemachineBasicMultiChannelPerlin? _perlin;

        private float _pitch;
        private float _pitchVel;
        private float _smoothedSpeed;
        private float _yaw;
        private float _yawVel;

        private void Awake()
        {
            _camera = GetComponent<CinemachineCamera>();
            _perlin = _camera.GetComponent<CinemachineBasicMultiChannelPerlin>();

            yawTarget ??= transform;
            pitchTarget ??= transform;
            speedTarget ??= yawTarget;

            _yaw = _desiredYaw = yawTarget.eulerAngles.y;
            _pitch = _desiredPitch = NormalizeAngle(pitchTarget.localEulerAngles.x);
            _desiredPitch = Mathf.Clamp(_desiredPitch, lookAngleMinMax.x, lookAngleMinMax.y);
            _pitch = _desiredPitch;
            _lastSpeedPos = speedTarget.position;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            CalculateRotation();
            SmoothRotation();
            ApplyRotation();
            UpdateHeadBob();
        }

        private void OnValidate()
        {
            _camera = GetComponent<CinemachineCamera>();
            _perlin = _camera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (!_perlin) _perlin = _camera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
            _perlin.AmplitudeGain = 0f;
            _perlin.FrequencyGain = 0f;
        }

        public void Look(Vector2 input)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
                input = Vector2.zero;

            _lookInput = input;
        }

        private void CalculateRotation()
        {
            _desiredYaw += _lookInput.x * xSensitivity * Time.deltaTime;
            _desiredYaw = Mathf.Repeat(_desiredYaw, 360f);

            _desiredPitch -= _lookInput.y * ySensitivity * Time.deltaTime;
            _desiredPitch = Mathf.Clamp(_desiredPitch, lookAngleMinMax.x, lookAngleMinMax.y);
        }

        private void SmoothRotation()
        {
            if (smoothTime <= 0f)
            {
                _yaw = _desiredYaw;
                _pitch = _desiredPitch;
                return;
            }

            _yaw = Mathf.SmoothDampAngle(_yaw, _desiredYaw, ref _yawVel, smoothTime);
            _pitch = Mathf.SmoothDampAngle(_pitch, _desiredPitch, ref _pitchVel, smoothTime);
        }

        private void ApplyRotation()
        {
            if (!yawTarget || !pitchTarget) return;
            yawTarget.rotation = Quaternion.Euler(0f, _yaw, 0f);
            pitchTarget.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateHeadBob()
        {
            if (!_perlin || !speedTarget)
                return;

            float dt = Time.deltaTime;
            if (dt <= 0f)
                return;

            // World-space speed from transform motion
            Vector3 pos = speedTarget.position;
            float rawSpeed = (pos - _lastSpeedPos).magnitude / dt;
            _lastSpeedPos = pos;

            // Smooth speed to avoid jitter
            _smoothedSpeed = Mathf.Lerp(
                _smoothedSpeed,
                rawSpeed,
                1f - Mathf.Exp(-bobResponse * dt)
            );

            var norm = 0f;
            if (_smoothedSpeed > IdleSpeed)
                norm = Mathf.InverseLerp(IdleSpeed, MaxSpeedForBob, _smoothedSpeed);

            float targetAmp =
                amplitudeBySpeed.Evaluate(norm) * amplitudeMultiplier;

            float targetFreq =
                frequencyBySpeed.Evaluate(norm) * frequencyMultiplier;

            _currentAmp = Mathf.Lerp(
                _currentAmp,
                targetAmp,
                1f - Mathf.Exp(-bobResponse * dt)
            );

            _currentFreq = Mathf.Lerp(
                _currentFreq,
                targetFreq,
                1f - Mathf.Exp(-bobResponse * dt)
            );

            _perlin.AmplitudeGain = _currentAmp;
            _perlin.FrequencyGain = _currentFreq;
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}