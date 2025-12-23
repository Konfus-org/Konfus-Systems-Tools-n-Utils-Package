using System;
using Konfus.Utility.Attributes;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class SetBehaviorEnabledEffect : Effect
    {
        [SerializeField]
        private bool isBehaviourEnabled;

        [SerializeField]
        [ComponentPicker(new[] { typeof(Behaviour) })]
        private Behaviour? behaviour;

        public override float Duration => 0;

        public override void Play()
        {
            if (!behaviour)
            {
                Debug.LogWarning($"{nameof(SetBehaviorEnabledEffect)} requires a behaviour.");
                return;
            }

            behaviour.enabled = isBehaviourEnabled;
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}