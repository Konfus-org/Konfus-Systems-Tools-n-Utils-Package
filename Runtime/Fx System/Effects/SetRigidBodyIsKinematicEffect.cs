using System;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class SetRigidBodyIsKinematicEffect : Effect
    {
        [SerializeField]
        private Rigidbody rigidbody;

        [SerializeField] 
        private bool isRigidBodyKinematic;

        public override float Duration => 0;

        public override void Play()
        {
            rigidbody.isKinematic = isRigidBodyKinematic;
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}