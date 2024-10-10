using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Systems.FX
{
    public class FxSystem : MonoBehaviour
    {
        [SerializeField] private List<FxItem> fxItems;

        private bool _isPlaying;

        public IReadOnlyList<FxItem> Items => fxItems;
        public UnityEvent finishedPlaying;

        public void PlayEffects()
        {
            if (_isPlaying) return;
            StartCoroutine(PlayEffectsCoroutine());
        }

        public void StopEffects()
        {
            if (_isPlaying) StopCoroutine(PlayEffectsCoroutine());
            _isPlaying = false;
        }

        private void Start()
        {
            foreach (FxItem fxItem in fxItems)
            {
                fxItem.Effect.Initialize(gameObject);
            }
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
        
        private void OnDestroy()
        {
            StopEffects();
        }

        private void OnDisable()
        {
            StopEffects();
        }
    }
}