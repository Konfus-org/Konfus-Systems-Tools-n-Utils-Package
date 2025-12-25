using UnityEngine;

namespace Konfus.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private float moveSpeed = 6f;

        [Tooltip("Max horizontal acceleration rate in m/s²")]
        [SerializeField]
        private float accelerationRate = 35f;
        [Tooltip("Max horizontal deceleration rate in m/s²")]
        [SerializeField]
        private float decelerationRate = 45f;

        [Tooltip("X = normalized speed 0..1, Y = multiplier")]
        [SerializeField]
        private AnimationCurve accelerationCurve =
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
        [Tooltip("X = normalized speed 0..1, Y = multiplier")]
        [SerializeField]
        private AnimationCurve decelerationCurve =
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0.6f);

        private Vector2 _moveInput;
        private Rigidbody? _rigidbody;

        private void FixedUpdate()
        {
            if (!_rigidbody) return;

            Vector3 currentVelocity = _rigidbody.linearVelocity;

            // ---- Horizontal movement (your original logic) ----
            var horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            var desiredDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);
            desiredDirection = Vector3.ClampMagnitude(desiredDirection, 1f);
            desiredDirection = transform.TransformDirection(desiredDirection);

            Vector3 desiredHorizontalVelocity = desiredDirection * moveSpeed;

            float speed = horizontalVelocity.magnitude;
            float normalizedSpeed = moveSpeed > 0.0001f
                ? Mathf.Clamp01(speed / moveSpeed)
                : 0f;

            bool isAccelerating =
                _moveInput.sqrMagnitude > 0.0001f &&
                desiredHorizontalVelocity.sqrMagnitude > horizontalVelocity.sqrMagnitude &&
                Vector3.Dot(desiredHorizontalVelocity, horizontalVelocity) >= 0f;

            float curveMultiplier = isAccelerating
                ? Mathf.Max(0f, accelerationCurve.Evaluate(normalizedSpeed))
                : Mathf.Max(0f, decelerationCurve.Evaluate(normalizedSpeed));

            float maxRate = isAccelerating ? accelerationRate : decelerationRate;
            float maxDeltaVelocity = maxRate * curveMultiplier * Time.fixedDeltaTime;

            Vector3 newHorizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredHorizontalVelocity,
                maxDeltaVelocity
            );

            _rigidbody.linearVelocity = new Vector3(
                newHorizontalVelocity.x,
                currentVelocity.y,
                newHorizontalVelocity.z
            );
        }

        private void OnValidate()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            EnsureCurveHasEndpoints(ref accelerationCurve);
            EnsureCurveHasEndpoints(ref decelerationCurve);
        }

        /// <summary>
        /// Supplies movement input in local X/Z space.
        /// Expected range: (-1..1) per axis.
        /// </summary>
        public void Move(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        private static void EnsureCurveHasEndpoints(ref AnimationCurve curve)
        {
            if (curve.length == 0)
            {
                curve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
                return;
            }

            float firstTime = curve.keys[0].time;
            float lastTime = curve.keys[curve.length - 1].time;
            float firstVal = curve.keys[0].value;
            float lastVal = curve.keys[curve.length - 1].value;

            if (firstTime > 0f)
                curve.AddKey(0f, curve.Evaluate(0f));
            if (lastTime < 1f)
                curve.AddKey(1f, curve.Evaluate(1f));
            if (firstVal <= 0f)
                curve.keys[0].value = 0.1f;
            if (lastVal >= 1f)
                curve.keys[curve.length - 1].value = 1;
        }
    }
}