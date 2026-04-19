using System;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class SetGameObjectActiveEffect : Effect
    {
        [SerializeField]
        private bool isGameObjectActive;

        [SerializeField]
        private GameObject? gameObject;

        private bool _previewOriginalActive;
        private bool _hasPreviewOriginalState;

        public override float Duration => 0;

        public override void Play()
        {
            if (!gameObject)
            {
                Debug.LogWarning($"{nameof(SetGameObjectActiveEffect)} requires a game object to be set.");
                return;
            }

            if (!_hasPreviewOriginalState)
            {
                _previewOriginalActive = gameObject.activeSelf;
                _hasPreviewOriginalState = true;
            }

            gameObject.SetActive(isGameObjectActive);
        }

        public override void Pause()
        {
            // do nothing...
        }

        public override void Reset()
        {
            if (!_hasPreviewOriginalState || !gameObject) return;
            gameObject.SetActive(_previewOriginalActive);
            _hasPreviewOriginalState = false;
        }
    }
}
