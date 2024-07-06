using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Systems.FX
{
    public class FxSystem : MonoBehaviour
    {
        [SerializeField]
        private List<FxItem> fxItems;

        private bool _isPlaying;
        private bool _isInitialized;

        public void PlayEffects()
        {
            if (_isPlaying) return;
            if (!_isInitialized) Initialize();
            StartCoroutine(PlayEffectsCoroutine());
        }

        private void Initialize()
        {
            foreach (FxItem fxItem in fxItems)
            {
                fxItem.Effect.Initialize(gameObject);
            }

            _isInitialized = true;
        }

        private IEnumerator PlayEffectsCoroutine()
        {
            _isPlaying = true;
            
            foreach (FxItem item in fxItems)
            {
                item.Effect.Play();
                float playTime = item.Effect.GetPlayTime();
                yield return new WaitForSeconds(playTime);
            }
            
            _isPlaying = false;
            yield return null;
        }

        private void OnDisable()
        {
            if (_isPlaying) StopCoroutine(PlayEffectsCoroutine());
            _isPlaying = false;
        }
    }
}