using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class DestroySelfEffect : Effect
    {
        private GameObject? _gameObjectToDestroy;
        private bool _previewOriginalActive;
        private bool _hasPreviewOriginalState;

        public override float Duration => 0;

        public override void Initialize(GameObject parentGo)
        {
            _gameObjectToDestroy = parentGo;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            if (!Application.isPlaying)
            {
                if (_gameObjectToDestroy == null) return;
                if (!_hasPreviewOriginalState)
                {
                    _previewOriginalActive = _gameObjectToDestroy.activeSelf;
                    _hasPreviewOriginalState = true;
                }

                _gameObjectToDestroy.SetActive(false);
                return;
            }

            Object.Destroy(_gameObjectToDestroy);
        }

        public override void Pause()
        {
            // Do nothing
        }

        public override void Reset()
        {
            if (Application.isPlaying || !_hasPreviewOriginalState || !_gameObjectToDestroy) return;
            _gameObjectToDestroy.SetActive(_previewOriginalActive);
            _hasPreviewOriginalState = false;
        }
    }
}
