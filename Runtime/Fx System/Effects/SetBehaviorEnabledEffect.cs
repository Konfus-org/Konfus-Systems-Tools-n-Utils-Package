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

        private bool _previewOriginalEnabled;
        private bool _hasPreviewOriginalState;

        public override float Duration => 0;

        public override void Play()
        {
            if (!behaviour)
            {
                Debug.LogWarning($"{nameof(SetBehaviorEnabledEffect)} requires a behaviour.");
                return;
            }

            if (!_hasPreviewOriginalState)
            {
                _previewOriginalEnabled = behaviour.enabled;
                _hasPreviewOriginalState = true;
            }

            behaviour.enabled = isBehaviourEnabled;
        }

        public override void Pause()
        {
            // do nothing...
        }

        public override void Reset()
        {
            if (!_hasPreviewOriginalState || !behaviour) return;
            behaviour.enabled = _previewOriginalEnabled;
            _hasPreviewOriginalState = false;
        }
    }
}
