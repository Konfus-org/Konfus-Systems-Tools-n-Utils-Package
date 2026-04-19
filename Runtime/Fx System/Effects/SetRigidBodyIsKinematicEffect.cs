using System;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class SetRigidBodyIsKinematicEffect : Effect
    {
        [SerializeField]
        private Rigidbody? rigidbody;

        [SerializeField]
        private bool isRigidBodyKinematic;

        private bool _previewOriginalKinematic;
        private bool _hasPreviewOriginalState;

        public override float Duration => 0;

        public override void Play()
        {
            if (!rigidbody)
            {
                Debug.LogWarning($"{nameof(SetRigidBodyIsKinematicEffect)} requires a rigidbody.");
                return;
            }

            if (!_hasPreviewOriginalState)
            {
                _previewOriginalKinematic = rigidbody.isKinematic;
                _hasPreviewOriginalState = true;
            }

            rigidbody.isKinematic = isRigidBodyKinematic;
        }

        public override void Pause()
        {
            // do nothing...
        }

        public override void Reset()
        {
            if (!_hasPreviewOriginalState || !rigidbody) return;
            rigidbody.isKinematic = _previewOriginalKinematic;
            _hasPreviewOriginalState = false;
        }
    }
}
