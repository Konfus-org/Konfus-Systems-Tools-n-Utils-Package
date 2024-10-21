using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Systems.FX
{
    public class FxSystem : MonoBehaviour
    {
        [Header("Settings:")]
        [SerializeField] private bool playOnAwake;
        
        [Header("Effects To Play:")]
        [SerializeField] private List<FxItem> fxItems;
        
        [Header("Events:")]
        public UnityEvent startedPlaying;
        public UnityEvent finishedPlaying;
        
        public IReadOnlyList<FxItem> Items => fxItems;

        private bool _isPlaying;

        public void PlayEffects()
        {
            if (_isPlaying) return;
            startedPlaying.Invoke();
            StartCoroutine(PlayEffectsCoroutine());
        }

        public void StopEffects()
        {
            if (_isPlaying) StopCoroutine(PlayEffectsCoroutine());
            foreach (FxItem fxItem in fxItems)
            {
                fxItem.Effect.IsPlaying = false;
                fxItem.Effect.Stop();
            }
            
            _isPlaying = false;
            finishedPlaying.Invoke();
        }

        private void Awake()
        {
            foreach (FxItem fxItem in fxItems)
            {
                fxItem.Effect.Initialize(gameObject);
            }
            
            if (playOnAwake) PlayEffects();
        }

        private IEnumerator PlayEffectsCoroutine()
        {
            _isPlaying = true;
            
            foreach (FxItem item in fxItems)
            {
                item.Effect.IsPlaying = true;
                item.Effect.Play();
                float playTime = item.Effect.Duration;
                yield return new WaitForSeconds(playTime);
                item.Effect.IsPlaying = false;
            }
            
            _isPlaying = false;
            finishedPlaying.Invoke();
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