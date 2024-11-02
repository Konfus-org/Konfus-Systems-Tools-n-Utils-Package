using System;
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
        [Tooltip("Called when the FxSystem is played")]
        public UnityEvent startedPlaying;
        [Tooltip("Called when the FxSystem is stopped either by finishing or by being stopped")]
        public UnityEvent stoppedPlaying;
        [Tooltip("Called when the FxSystem has finished playing all effects")]
        public UnityEvent finishedPlaying;
        
        public IReadOnlyList<FxItem> Items => fxItems;

        public bool IsPlaying => _isPlaying;
        private bool _isPlaying;
        private bool _isStopping;

        public void PlayEffects()
        {
            if (_isPlaying) return;
            startedPlaying.Invoke();
            StartCoroutine(PlayEffectsCoroutine());
        }

        public void StopEffects()
        {
            _isStopping = true;
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
            stoppedPlaying.Invoke();
            _isStopping = false;
        }

        private void Awake()
        {
            foreach (FxItem fxItem in fxItems)
            {
                if (fxItem.Effect is MultiEffect multiEffect && multiEffect.FxSystem != null)
                {
                    multiEffect.IsPlaying = multiEffect.FxSystem.playOnAwake;
                }
                
                fxItem.Effect.Initialize(gameObject);
            }

            if (loopForever) finishedPlaying.AddListener(PlayEffects);
            if (playOnAwake) PlayEffects();
        }

        private IEnumerator PlayEffectsCoroutine()
        {
            _isPlaying = true;
            
            if (_isStopping) yield return null;
            
            foreach (FxItem item in fxItems)
            {
                if (_isStopping) yield return null;
                if (item.Effect is null) continue;
                
                if (item.Effect.ShouldPlayAsync && !item.Effect.IsPlaying)
                {
                    StartCoroutine(PlayEffectRoutine(item));
                }
                else
                {
                    yield return PlayEffectRoutine(item);
                }
            }
            
            if (_isStopping) yield return null;
            
            _isPlaying = false;
            stoppedPlaying.Invoke();
            finishedPlaying.Invoke();
            yield return null;
        }

        private IEnumerator PlayEffectRoutine(FxItem item)
        {
            item.Effect.IsPlaying = true;
            item.Effect.Play();
            
            if (item.Effect is MultiEffect multiEffect && 
                (multiEffect.FxSystem?.loopForever ?? false))
            {
                // If we play forever, set is playing to true and bail out...
                yield break;
            }
            
            yield return new WaitForSeconds(item.Effect.Duration);
            item.Effect.IsPlaying = false;
        }

        private void OnValidate()
        {
            foreach (var item in fxItems)
            {
                item.Effect?.OnValidate();
            }
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