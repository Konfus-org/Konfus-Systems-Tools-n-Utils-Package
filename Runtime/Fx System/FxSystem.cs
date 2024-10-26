using System.Collections;
using System.Collections.Generic;
using Konfus.Systems.Fx_System.Effects;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Systems.Fx_System
{
    public class FxSystem : MonoBehaviour
    {
        [Header("Settings:")]
        [SerializeField] private bool playOnAwake;
        [SerializeField] private bool loopForever;
        
        [Header("Effects To Play:")]
        [SerializeField] private List<FxItem> fxItems;
        
        [Header("Events:")]
        public UnityEvent startedPlaying;
        public UnityEvent finishedPlaying;
        
        public IReadOnlyList<FxItem> Items => fxItems;

        public bool IsPlaying => _isPlaying;
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
                if (fxItem.Effect.IsPlaying && fxItem.Effect.ShouldPlayAsync)
                {
                    StopCoroutine(PlayEffectRoutine(fxItem));
                }
                
                fxItem.Effect.IsPlaying = false;
                fxItem.Effect.Stop();
            }
            
            _isPlaying = false;
            finishedPlaying.RemoveListener(PlayEffects);
            finishedPlaying.Invoke();
        }

        private void Awake()
        {
            foreach (FxItem fxItem in fxItems)
            {
                fxItem.Effect.Initialize(gameObject);
            }

            if (loopForever) finishedPlaying.AddListener(PlayEffects);
            if (playOnAwake) PlayEffects();
        }

        private IEnumerator PlayEffectsCoroutine()
        {
            _isPlaying = true;
            
            foreach (FxItem item in fxItems)
            {
                if (item.Effect.ShouldPlayAsync && !item.Effect.IsPlaying)
                {
                    StartCoroutine(PlayEffectRoutine(item));
                }
                else
                {
                    yield return PlayEffectRoutine(item);
                }
            }
            
            _isPlaying = false;
            finishedPlaying.Invoke();
            yield return null;
        }

        private IEnumerator PlayEffectRoutine(FxItem item)
        {
            item.Effect.IsPlaying = true;
            if (item.Effect is MultiEffect multiEffect && multiEffect)
            {
                
            }
            item.Effect.Play();
            yield return new WaitForSeconds(item.Effect.Duration);
            item.Effect.IsPlaying = false;
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