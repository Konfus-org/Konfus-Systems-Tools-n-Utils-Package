using UnityEngine;

namespace Konfus.Movement
{
    [DisallowMultipleComponent]
    public class Movable : MonoBehaviour
    {
        [SerializeField]
        private Transform? motionReference;
        [SerializeField]
        private Rigidbody? body;

        public Transform MotionReference => motionReference != null ? motionReference : transform;
        public Rigidbody? Body => body;

        private void Awake()
        {
            body ??= GetComponentInParent<Rigidbody>();
        }
    }
}
