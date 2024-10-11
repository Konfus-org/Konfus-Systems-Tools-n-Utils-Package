using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class SetRigidBodyIsKinematicEffect : Effect
    {
        [SerializeField]
        private Rigidbody rigidbody;

        [SerializeField] 
        private bool isKinematic;

        public override void Play()
        {
            rigidbody.isKinematic = isKinematic;
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}