using Konfus.Movement;
using Konfus.Sensor_Toolkit;
using UnityEngine;

namespace Konfus.Input
{
    [DisallowMultipleComponent]
    public class StickToMovable : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody? body;
        [SerializeField]
        private ScanSensor? groundSensor;
        [SerializeField]
        private RigidbodyJumping? jumping;
        [SerializeField]
        private bool inheritRotation = true;

        private Movable? _currentMovable;
        private Transform? _target;
        private Vector3 _lastPosition;
        private Quaternion _lastRotation = Quaternion.identity;

        public Movable? CurrentMovable => _currentMovable;
        public bool IsStuck => _target != null;

        private void Awake()
        {
            body ??= GetComponent<Rigidbody>();
            groundSensor ??= GetComponent<RigidbodyMovement>()?.GroundSensor;
            jumping ??= GetComponent<RigidbodyJumping>();
        }

        private void FixedUpdate()
        {
            RefreshAttachment();

            if (_target == null)
            {
                return;
            }

            Vector3 deltaPosition = _target.position - _lastPosition;
            Quaternion deltaRotation = _target.rotation * Quaternion.Inverse(_lastRotation);

            if (body != null)
            {
                body.MovePosition(body.position + deltaPosition);
                if (inheritRotation)
                {
                    body.MoveRotation(deltaRotation * body.rotation);
                }
            }
            else
            {
                transform.position += deltaPosition;
                if (inheritRotation)
                {
                    transform.rotation = deltaRotation * transform.rotation;
                }
            }

            _lastPosition = _target.position;
            _lastRotation = _target.rotation;
        }

        public void Stick(Movable movable)
        {
            if (movable == null)
            {
                return;
            }

            _currentMovable = movable;
            _target = movable.MotionReference;
            _lastPosition = _target.position;
            _lastRotation = _target.rotation;
        }

        public void Unstick()
        {
            _currentMovable = null;
            _target = null;
        }

        private void RefreshAttachment()
        {
            if (jumping != null && !jumping.IsGroundedNow)
            {
                Unstick();
                return;
            }

            Movable? nextMovable = FindGroundMovable();
            if (nextMovable == _currentMovable)
            {
                return;
            }

            if (nextMovable == null)
            {
                Unstick();
                return;
            }

            Stick(nextMovable);
        }

        private Movable? FindGroundMovable()
        {
            if (groundSensor == null || !groundSensor.Scan() || groundSensor.Hits == null)
            {
                return null;
            }

            Movable? bestMovable = null;
            float bestUpDot = float.NegativeInfinity;

            foreach (Sensor.Hit hit in groundSensor.Hits)
            {
                if (!hit.GameObject || hit.GameObject == gameObject || hit.GameObject.transform.IsChildOf(transform))
                {
                    continue;
                }

                Movable? movable = hit.GameObject.GetComponentInParent<Movable>();
                if (movable == null)
                {
                    continue;
                }

                float upDot = Vector3.Dot(hit.Normal.normalized, Vector3.up);
                if (upDot <= 0f || upDot <= bestUpDot)
                {
                    continue;
                }

                bestUpDot = upDot;
                bestMovable = movable;
            }

            return bestMovable;
        }
    }
}
