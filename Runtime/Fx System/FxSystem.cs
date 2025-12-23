using System.Collections;
using System.Collections.Generic;
using Konfus.Fx_System.Effects;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Fx_System
{
    public class FxSystem : MonoBehaviour
    {
        [SerializeField]
        private bool playOnAwake;

        [SerializeField]
        private bool loopForever;

        [SerializeField]
        private List<FxItem> fxItems = new();

        [Tooltip("Called when the FxSystem is played")]
        public UnityEvent? startedPlaying;

        [Tooltip("Called when the FxSystem is stopped either by finishing or by being stopped")]
        public UnityEvent? stoppedPlaying;

        [Tooltip("Called when the FxSystem has finished playing all effects")]
        public UnityEvent? finishedPlaying;

        public IReadOnlyList<FxItem> Items => fxItems;

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            foreach (FxItem fxItem in fxItems)
            {
                if (fxItem.Effect is MultiEffect multiEffect && multiEffect.FxSystem != null)
                    multiEffect.IsPlaying = multiEffect.FxSystem.playOnAwake;

                fxItem.Effect?.Initialize(gameObject);
            }

            if (loopForever) finishedPlaying?.AddListener(PlayEffects);
            if (playOnAwake) PlayEffects();
        }

        private void OnDisable()
        {
            StopEffects();
        }

        private void OnDestroy()
        {
            StopEffects();
        }

        private void OnValidate()
        {
            foreach (FxItem? item in fxItems)
            {
                item.Effect?.OnValidate();
            }
        }

        public void PlayEffects()
        {
            if (IsPlaying) return;
            startedPlaying?.Invoke();
            StartCoroutine(PlayEffectsCoroutine());
        }

        public void StopEffects()
        {
            StopAllCoroutines();
            foreach (FxItem fxItem in fxItems)
            {
                if (fxItem == null || fxItem.Effect == null) continue;

                fxItem.Effect.IsPlaying = false;
                fxItem.Effect.Stop();
            }

            IsPlaying = false;
            stoppedPlaying?.Invoke();
        }

        private IEnumerator PlayEffectsCoroutine()
        {
            IsPlaying = true;

            foreach (FxItem fxItem in fxItems)
            {
                if (fxItem?.Effect == null) continue;

                if (fxItem.Effect.ShouldPlayAsync && !fxItem.Effect.IsPlaying)
                    StartCoroutine(PlayEffectRoutine(fxItem));
                else
                    yield return PlayEffectRoutine(fxItem);
            }

            IsPlaying = false;
            stoppedPlaying?.Invoke();
            finishedPlaying?.Invoke();
            yield return null;
        }

        private static IEnumerator PlayEffectRoutine(FxItem item)
        {
            if (item.Effect == null) yield break;

            item.Effect.IsPlaying = true;
            item.Effect.Play();

            if (item.Effect is MultiEffect multiEffect &&
                (multiEffect.FxSystem?.loopForever ?? false))
                // If we play forever, set is playing to true and bail out...
                yield break;

            yield return new WaitForSeconds(item.Effect.Duration);
            item.Effect.IsPlaying = false;
        }
    }
}