using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class DestroyEffect : Effect
    {
        [SerializeField]
        private GameObject? gameObjectToDestroy;
        private bool _previewOriginalActive;
        private bool _hasPreviewOriginalState;

        public override float Duration => 0;

        public override void Play()
        {
            if (gameObjectToDestroy == null)
            {
                Debug.Log($"{nameof(DestroyEffect)} requires a game object to destroy");
                return;
            }

            if (!Application.isPlaying)
            {
                if (!_hasPreviewOriginalState)
                {
                    _previewOriginalActive = gameObjectToDestroy.activeSelf;
                    _hasPreviewOriginalState = true;
                }

                gameObjectToDestroy.SetActive(false);
                return;
            }

            Object.Destroy(gameObjectToDestroy);
        }

        public override void Pause()
        {
            // do nothing...
        }

        public override void Reset()
        {
            if (Application.isPlaying || !_hasPreviewOriginalState || !gameObjectToDestroy) return;
            gameObjectToDestroy.SetActive(_previewOriginalActive);
            _hasPreviewOriginalState = false;
        }
    }
}
