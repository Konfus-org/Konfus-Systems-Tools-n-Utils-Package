using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class EnableDisableColliderEffect : Effect
    {
        [SerializeField]
        private Collider collider;

        [SerializeField] 
        private bool enabled;
        
        public override void Play()
        {
            collider.enabled = enabled;
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}