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

        [Header("Look")]
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
        [MinMaxRangeSlider(-90f, 90f)]
        private Vector2 lookAngleMinMax = new(-80f, 85f);

        [Header("Head Bob")]
        [SerializeField]
        private AnimationCurve amplitudeBySpeed =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField]
        private AnimationCurve frequencyBySpeed =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
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
        [Tooltip("Max camera roll (degrees) at full lean input.")]
        private float maxLeanAngle = 12f;
        [SerializeField]
        [Tooltip("How quickly lean reacts (seconds). 0 = instant.")]
        [Range(0f, 0.25f)]
        private float leanSmoothTime = 0.08f;
        [SerializeField]
        [Tooltip("Deadzone for lean input.")]
        [Range(0f, 0.5f)]
        private float leanDeadzone = 0.05f;
        [SerializeField]
        [Tooltip("Also lean based on strafing speed (world motion).")]
        private bool leanFromStrafeSpeed = true;
        [SerializeField]
        [Min(0)]
        [Tooltip("Strafe speed (m/s) that maps to full lean when leaning from strafe speed.")]
        private float maxStrafeSpeedForLean = 5f;

        [SerializeField]
        [Tooltip("How much of the lean comes from strafe speed (0..1).")]
        [Range(0f, 1f)]
        private float strafeLeanWeight = 0.65f;

        private CinemachineCamera? _camera;

        private float _desiredPitch;
        private float _desiredRoll;
        private float _desiredYaw;

        private Vector3 _lastSpeedPos;

        // Lean state
        private float _leanInput;
        private Vector2 _lookInput;

        private CinemachineBasicMultiChannelPerlin? _perlin;
        private float _perlinCurrentAmp;
        private float _perlinCurrentFreq;

        private float _pitch;
        private float _pitchVel;
        private float _roll;
        private float _rollVel;
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

            // Initialize roll from current pitchTarget roll (signed), in case prefab starts rolled.
            _roll = _desiredRoll = NormalizeAngle(pitchTarget.localEulerAngles.z);

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

        /// <summary>
        /// Lean left (-1) or right (1). Expected range [-1..1].
        /// </summary>
        public void Lean(float input)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
                input = 0f;

            // Deadzone + clamp.
            if (Mathf.Abs(input) < leanDeadzone) input = 0f;
            _leanInput = Mathf.Clamp(input, -1f, 1f);
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
            // Base lean comes from player input.
            float inputTarget = _leanInput;

            // Optional: add lean from actual strafe velocity (measured from speedTarget motion).
            if (leanFromStrafeSpeed && speedTarget && yawTarget)
            {
                float dt = Time.deltaTime;
                if (dt > 0f)
                {
                    Vector3 pos = speedTarget.position;
                    Vector3 worldVel = (pos - _lastSpeedPos) / dt;

                    // Strafe speed = velocity along yawTarget's right axis.
                    float strafeSpeed = Vector3.Dot(worldVel, yawTarget.right);

                    var strafeNorm = 0f;
                    if (maxStrafeSpeedForLean > 0f)
                        strafeNorm = Mathf.Clamp(strafeSpeed / maxStrafeSpeedForLean, -1f, 1f);

                    // Blend: input dominates, strafe adds “physicality”.
                    inputTarget = Mathf.Lerp(inputTarget, strafeNorm, strafeLeanWeight);
                }
            }

            _desiredRoll = -inputTarget * maxLeanAngle; // negative so leaning right rolls right (typical FPS feel)
        }

        private void SmoothRotation()
        {
            if (smoothTime <= 0f)
            {
                _yaw = _desiredYaw;
                _pitch = _desiredPitch;
            }
            else
            {
                _yaw = Mathf.SmoothDampAngle(_yaw, _desiredYaw, ref _yawVel, smoothTime);
                _pitch = Mathf.SmoothDampAngle(_pitch, _desiredPitch, ref _pitchVel, smoothTime);
            }

            // Lean smoothing can be separate so you can make it snappier/slower than look.
            if (leanSmoothTime <= 0f)
                _roll = _desiredRoll;
            else
                _roll = Mathf.SmoothDampAngle(_roll, _desiredRoll, ref _rollVel, leanSmoothTime);
        }

        private void ApplyRotation()
        {
            if (!yawTarget || !pitchTarget) return;

            yawTarget.rotation = Quaternion.Euler(0f, _yaw, 0f);

            // Apply pitch + roll together on pitch pivot.
            pitchTarget.localRotation = Quaternion.Euler(_pitch, 0f, _roll);
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

            // Smooth speed to avoid jitter
            _smoothedSpeed = Mathf.Lerp(
                _smoothedSpeed,
                rawSpeed,
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

            // IMPORTANT: _lastSpeedPos is used by BOTH bob + lean-from-strafe.
            // We only update it here once per frame, after both have sampled it.
            _lastSpeedPos = pos;
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}