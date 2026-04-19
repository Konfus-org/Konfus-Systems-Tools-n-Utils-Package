using System;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class SetSelfActiveEffect : Effect
    {
        [SerializeField]
        private bool active;

        private GameObject? _go;
        private bool _previewOriginalActive;
        private bool _hasPreviewOriginalState;

        public override float Duration => 0;

        public override void Initialize(GameObject parentGo)
        {
            _go = parentGo;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            if (!_go)
            {
                Debug.LogWarning($"{nameof(SetGameObjectActiveEffect)} requires a game object to be set.");
                return;
            }

            if (!_hasPreviewOriginalState)
            {
                _previewOriginalActive = _go.activeSelf;
                _hasPreviewOriginalState = true;
            }

            _go.SetActive(active);
        }

        public override void Pause()
        {
            // do nothing...
        }

        public override void Reset()
        {
            if (!_hasPreviewOriginalState || !_go) return;
            _go.SetActive(_previewOriginalActive);
            _hasPreviewOriginalState = false;
        }
    }
}
