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
        private const float StrafeLeanWeight = 0.65f;

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

        [Header("Look")]
        [SerializeField]
        [Range(0f, 100f)]
        private float xSensitivity = 50f;
        [SerializeField]
        [Range(0f, 100f)]
        private float ySensitivity = 50f;
        [SerializeField]
        [Range(0f, 0.25f)]
        private float lookSmoothing = 0.06f;
        [SerializeField]
        [MinMaxRangeSlider(-90f, 90f)]
        private Vector2 lookAngleMinMax = new(-80f, 85f);

        [Header("Head Bob")]
        [SerializeField]
        private AnimationCurve amplitudeBySpeed = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField]
        private AnimationCurve frequencyBySpeed = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField]
        [Min(0)]
        private float amplitudeMultiplier = 1f;
        [SerializeField]
        [Min(0)]
        private float frequencyMultiplier = 1f;
        [SerializeField]
        [Tooltip("How quickly bob reacts to speed changes.")]
        [Min(0)]
        private float bobResponse = 12f;

        [Header("Lean")]
        [SerializeField]
        [Min(0)]
        [Tooltip("Max camera roll (degrees) at full strafe lean.")]
        private float maxStrafeLean = 2f;
        [SerializeField]
        [Min(0)]
        [Tooltip("Max camera pitch offset (degrees) from forward/back movement.")]
        private float maxForwardLean = 1f;
        [SerializeField]
        [Tooltip("How quickly lean reacts (seconds). 0 = instant.")]
        [Range(0f, 0.25f)]
        private float leanSmoothing = 0.08f;
        [SerializeField]
        [Min(0)]
        [Tooltip("Strafe speed (m/s) that maps to full roll lean.")]
        private float maxStrafeSpeedForLean = 3f;
        [SerializeField]
        [Min(0)]
        [Tooltip("Forward speed (m/s) that maps to full forward/back tilt.")]
        private float maxForwardSpeedForLean = 4f;

        private CinemachineCamera? _camera;

        private float _desiredMovePitch; // pitch offset from movement

        private float _desiredPitch;
        private float _desiredRoll; // Z
        private float _desiredYaw;

        // Speed sampling (single source of truth per frame)
        private Vector3 _lastSpeedPos;

        // Lean state
        private float _leanInput; // -1..1 (strafe input)

        private Vector2 _lookInput;
        private float _movePitch;
        private float _movePitchVel;

        private CinemachineBasicMultiChannelPerlin? _perlin;
        private float _perlinCurrentAmp;
        private float _perlinCurrentFreq;

        // Look smoothing state
        private float _pitch;
        private float _pitchVel;
        private float _rawSpeed;
        private float _roll;
        private float _rollVel;
        private float _smoothedSpeed;
        private Vector3 _worldVelocity;
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

            _roll = _desiredRoll = NormalizeAngle(pitchTarget.localEulerAngles.z);
            _movePitch = _desiredMovePitch = 0f;

            _lastSpeedPos = speedTarget.position;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            SampleVelocity(); // <-- compute _worldVelocity/_rawSpeed once per frame

            CalculateRotation(); // includes CalculateLean() using sampled velocity
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

        /// <summary>
        /// Strafe lean left (-1) or right (1). Expected range [-1..1].
        /// </summary>
        public void Lean(float input)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
                input = 0f;

            _leanInput = Mathf.Clamp(input, -1f, 1f);
        }

        private void SampleVelocity()
        {
            if (!speedTarget)
            {
                _worldVelocity = Vector3.zero;
                _rawSpeed = 0f;
                return;
            }

            float dt = Time.deltaTime;
            if (dt <= 0f)
                return;

            Vector3 pos = speedTarget.position;
            _worldVelocity = (pos - _lastSpeedPos) / dt;
            _rawSpeed = _worldVelocity.magnitude;

            _lastSpeedPos = pos;
        }

        private void CalculateRotation()
        {
            _desiredYaw += _lookInput.x * xSensitivity * Time.deltaTime;
            _desiredYaw = Mathf.Repeat(_desiredYaw, 360f);

            _desiredPitch -= _lookInput.y * ySensitivity * Time.deltaTime;
            _desiredPitch = Mathf.Clamp(_desiredPitch, lookAngleMinMax.x, lookAngleMinMax.y);

            CalculateLean();
        }

        private void CalculateLean()
        {
            if (!yawTarget)
            {
                _desiredRoll = 0f;
                _desiredMovePitch = 0f;
                return;
            }

            // ----- ROLL (Z) from strafe input + optional strafe-speed influence
            if (maxStrafeSpeedForLean > 0f)
            {
                float rollInput = _leanInput;
                float strafeSpeed = Vector3.Dot(_worldVelocity, yawTarget.right);
                float strafeNorm = Mathf.Clamp(strafeSpeed / maxStrafeSpeedForLean, -1f, 1f);
                rollInput = Mathf.Lerp(rollInput, strafeNorm, StrafeLeanWeight);
                _desiredRoll = -rollInput * maxStrafeLean;
            }

            // ----- MOVE PITCH (X offset) from forward/back speed
            if (maxForwardSpeedForLean > 0f)
            {
                float forwardSpeed = Vector3.Dot(_worldVelocity, yawTarget.forward);
                float forwardNorm = Mathf.Clamp(forwardSpeed / maxForwardSpeedForLean, -1f, 1f);

                // Typical feel: tilt forward when moving forward (camera looks slightly down),
                // tilt back when moving backward (camera looks slightly up).
                _desiredMovePitch = forwardNorm * maxForwardLean;
            }
        }

        private void SmoothRotation()
        {
            if (lookSmoothing <= 0f)
            {
                _yaw = _desiredYaw;
                _pitch = _desiredPitch;
            }
            else
            {
                _yaw = Mathf.SmoothDampAngle(_yaw, _desiredYaw, ref _yawVel, lookSmoothing);
                _pitch = Mathf.SmoothDampAngle(_pitch, _desiredPitch, ref _pitchVel, lookSmoothing);
            }

            if (leanSmoothing <= 0f)
            {
                _roll = _desiredRoll;
                _movePitch = _desiredMovePitch;
            }
            else
            {
                _roll = Mathf.SmoothDampAngle(_roll, _desiredRoll, ref _rollVel, leanSmoothing);
                _movePitch = Mathf.SmoothDampAngle(_movePitch, _desiredMovePitch, ref _movePitchVel, leanSmoothing);
            }
        }

        private void ApplyRotation()
        {
            if (!yawTarget || !pitchTarget) return;

            yawTarget.rotation = Quaternion.Euler(0f, _yaw, 0f);

            // Apply mouse pitch + movement pitch offset + roll together on pitch pivot.
            pitchTarget.localRotation = Quaternion.Euler(_pitch + _movePitch, 0f, _roll);
        }

        private void UpdateHeadBob()
        {
            if (!_perlin)
                return;

            float dt = Time.deltaTime;
            if (dt <= 0f)
                return;

            _smoothedSpeed = Mathf.Lerp(
                _smoothedSpeed,
                _rawSpeed,
                1f - Mathf.Exp(-bobResponse * dt)
            );

            var norm = 0f;
            if (_smoothedSpeed > IdleSpeed)
                norm = Mathf.InverseLerp(IdleSpeed, MaxSpeedForBob, _smoothedSpeed);

            float targetAmp = amplitudeBySpeed.Evaluate(norm) * amplitudeMultiplier;
            float targetFreq = frequencyBySpeed.Evaluate(norm) * frequencyMultiplier;

            _perlinCurrentAmp = Mathf.Lerp(
                _perlinCurrentAmp,
                targetAmp,
                1f - Mathf.Exp(-bobResponse * dt)
            );

            _perlinCurrentFreq = Mathf.Lerp(
                _perlinCurrentFreq,
                targetFreq,
                1f - Mathf.Exp(-bobResponse * dt)
            );

            _perlin.AmplitudeGain = _perlinCurrentAmp;
            _perlin.FrequencyGain = _perlinCurrentFreq;
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}