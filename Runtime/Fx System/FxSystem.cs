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
        [InspectorName("Effects")]
        private List<FxItem> fxItems = new();

        [Tooltip("Called when the FxSystem is played")]
        public UnityEvent? startedPlaying;

        [Tooltip("Called when the FxSystem is stopped either by finishing or by being stopped")]
        public UnityEvent? stoppedPlaying;

        [Tooltip("Called when the FxSystem has finished playing all effects")]
        public UnityEvent? finishedPlaying;

        public IReadOnlyList<FxItem> Items => fxItems;

        public bool LoopForever => loopForever;

        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }

        private readonly List<PlayingEffect> _playingEffects = new();
        private int _nextItemIndex;
        private float _nextSequentialStartTime;
        private float _elapsedTime;
        private bool _hasPlaybackState;

        private void Awake()
        {
            InitializeEffects();

            if (!Application.isPlaying) return;

            if (loopForever) finishedPlaying?.AddListener(PlayEffects);
            if (playOnAwake) PlayEffects();
        }

        private void Update()
        {
            if (!Application.isPlaying || !IsPlaying || IsPaused || !_hasPlaybackState) return;
            TickRuntimePlayback(Time.deltaTime);
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

        public bool TryBeginPlayback()
        {
            if (IsPlaying || (IsPaused && _hasPlaybackState)) return false;

            InitializeEffects();
            PrepareForFreshPlayback();
            startedPlaying?.Invoke();

            IsPlaying = true;
            IsPaused = false;
            _hasPlaybackState = true;
            _nextItemIndex = 0;
            _nextSequentialStartTime = 0f;
            _elapsedTime = 0f;
            _playingEffects.Clear();

            return true;
        }

        public void PlayEffects()
        {
            if (!Application.isPlaying) return;

            if (IsPaused && _hasPlaybackState)
            {
                ResumeEffects();
                return;
            }

            if (!TryBeginPlayback()) return;
            TickRuntimePlayback(0f);
        }

        public void PauseEffects()
        {
            if (!_hasPlaybackState && !IsPlaying) return;

            PauseEffectsState();
            IsPlaying = false;
            IsPaused = true;
        }

        public void ResumeEffects()
        {
            if (!_hasPlaybackState || !IsPaused) return;

            ResumeEffectsState();
            IsPaused = false;
            IsPlaying = true;
        }

        public void ResetEffects()
        {
            bool wasActive = _hasPlaybackState || IsPlaying || IsPaused;

            PauseEffectsState();
            ResetEffectsState();
            ClearPlaybackState();

            if (wasActive)
                stoppedPlaying?.Invoke();
        }

        public void StopEffects()
        {
            bool wasActive = _hasPlaybackState || IsPlaying || IsPaused;

            PauseEffectsState();
            ClearPlaybackState();

            if (wasActive)
                stoppedPlaying?.Invoke();
        }

        private void InitializeEffects()
        {
            foreach (FxItem fxItem in fxItems)
            {
                if (fxItem == null || fxItem.Effect == null) continue;

                if (fxItem.Effect is MultiEffect multiEffect && multiEffect.FxSystem != null)
                    multiEffect.IsPlaying = multiEffect.FxSystem.playOnAwake;

                fxItem.Effect.Initialize(gameObject);
            }
        }

        private void TickRuntimePlayback(float deltaTime)
        {
            _elapsedTime += Mathf.Max(0f, deltaTime);
            StartDueRuntimeEffects();

            for (int i = _playingEffects.Count - 1; i >= 0; i--)
            {
                PlayingEffect playingEffect = _playingEffects[i];
                if (playingEffect.Effect.IsPlaying)
                    playingEffect.Effect.Tick(deltaTime);

                if (playingEffect.EndTime > _elapsedTime) continue;

                playingEffect.Effect.IsPlaying = false;
                _playingEffects.RemoveAt(i);
            }

            if (_nextItemIndex < fxItems.Count || _playingEffects.Count != 0) return;

            FinishPlayback();
        }

        private void StartDueRuntimeEffects()
        {
            while (_nextItemIndex < fxItems.Count && _elapsedTime >= _nextSequentialStartTime)
            {
                FxItem fxItem = fxItems[_nextItemIndex++];
                Effect? effect = fxItem?.Effect;
                if (effect == null) continue;

                effect.IsPlaying = true;
                effect.Play();

                bool isLoopingMultiEffect = effect is MultiEffect multiEffect &&
                                            (multiEffect.FxSystem?.LoopForever ?? false);

                if (!isLoopingMultiEffect)
                {
                    float duration = Mathf.Max(0f, effect.Duration);
                    _playingEffects.Add(new PlayingEffect(effect, _elapsedTime + duration));
                }

                if (effect.ShouldPlayAsync) continue;

                if (isLoopingMultiEffect)
                {
                    _nextSequentialStartTime = _elapsedTime;
                    continue;
                }

                float effectDuration = Mathf.Max(0f, effect.Duration);
                _nextSequentialStartTime = _elapsedTime + effectDuration;
                if (effectDuration > 0f)
                    break;
            }
        }

        private void PauseEffectsState()
        {
            foreach (FxItem fxItem in fxItems)
            {
                Effect? effect = fxItem?.Effect;
                if (effect == null || !effect.IsPlaying) continue;
                effect.Pause();
            }
        }

        private void ResumeEffectsState()
        {
            foreach (FxItem fxItem in fxItems)
            {
                Effect? effect = fxItem?.Effect;
                if (effect == null || !effect.IsPlaying) continue;
                effect.Resume();
            }
        }

        private void ResetEffectsState()
        {
            for (int i = fxItems.Count - 1; i >= 0; i--)
            {
                FxItem fxItem = fxItems[i];
                if (fxItem == null || fxItem.Effect == null) continue;

                fxItem.Effect.IsPlaying = false;
                fxItem.Effect.Reset();
            }
        }

        private void PrepareForFreshPlayback()
        {
            // Fresh runtime playback should start from the scene's current state.
            // Explicit ResetEffects is the destructive path that rewinds effects to baseline.
            PauseEffectsState();
            ClearPlaybackState();
        }

        private void ClearPlaybackState()
        {
            IsPlaying = false;
            IsPaused = false;
            _hasPlaybackState = false;
            _elapsedTime = 0f;
            _nextItemIndex = 0;
            _nextSequentialStartTime = 0f;
            _playingEffects.Clear();
        }

        private void FinishPlayback()
        {
            bool wasActive = _hasPlaybackState || IsPlaying || IsPaused;
            ClearPlaybackState();

            if (wasActive)
                stoppedPlaying?.Invoke();

            finishedPlaying?.Invoke();
        }

        private readonly struct PlayingEffect
        {
            public PlayingEffect(Effect effect, float endTime)
            {
                Effect = effect;
                EndTime = endTime;
            }

            public Effect Effect { get; }
            public float EndTime { get; }
        }
    }
}
