using System.Collections.Generic;
using DG.Tweening;
using Konfus.Fx_System;
using Konfus.Fx_System.Effects;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Fx_System
{
    [InitializeOnLoad]
    internal static class FxSystemPreviewController
    {
        private static readonly Dictionary<FxSystem, PreviewState> States = new();
        private static bool _isRegistered;
        private static double _lastGlobalTickTime;

        static FxSystemPreviewController()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static bool IsPreviewing(FxSystem fxSystem)
        {
            return fxSystem && States.ContainsKey(fxSystem);
        }

        public static void PlayPreview(FxSystem fxSystem)
        {
            if (!fxSystem || Application.isPlaying) return;

            if (States.TryGetValue(fxSystem, out PreviewState state))
            {
                if (state.IsPaused)
                {
                    state.IsPaused = false;
                    fxSystem.ResumeEffects();
                    EnsureUpdateRegistered();
                    SceneView.RepaintAll();
                    return;
                }

                StopPreview(fxSystem);
            }
            else if (fxSystem.IsPlaying || fxSystem.IsPaused)
            {
                StopPreview(fxSystem);
            }

            if (!fxSystem.TryBeginPlayback()) return;

            States[fxSystem] = new PreviewState(fxSystem);

            EnsureUpdateRegistered();
            TickPreview(fxSystem, States[fxSystem], 0f);
            SceneView.RepaintAll();
        }

        public static void PausePreview(FxSystem fxSystem)
        {
            if (!fxSystem) return;

            if (!States.TryGetValue(fxSystem, out PreviewState state)) return;

            state.IsPaused = true;
            fxSystem.PauseEffects();

            if (!HasActivePreviewStates())
                StopUpdateLoop();
        }

        public static void StopPreview(FxSystem fxSystem)
        {
            if (!fxSystem) return;

            States.Remove(fxSystem);
            fxSystem.StopEffects();

            if (States.Count == 0)
                StopUpdateLoop();
        }

        public static void ResetPreview(FxSystem fxSystem)
        {
            if (!fxSystem) return;

            States.Remove(fxSystem);
            fxSystem.ResetEffects();

            if (States.Count == 0)
                StopUpdateLoop();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode || States.Count == 0) return;

            List<FxSystem> previewSystems = new(States.Keys);
            for (int i = 0; i < previewSystems.Count; i++)
                ResetPreview(previewSystems[i]);
        }

        private static void EnsureUpdateRegistered()
        {
            if (_isRegistered) return;

            _lastGlobalTickTime = EditorApplication.timeSinceStartup;
            _isRegistered = true;
            EditorApplication.update += Update;
        }

        private static void StopUpdateLoop()
        {
            if (!_isRegistered) return;

            _isRegistered = false;
            EditorApplication.update -= Update;
        }

        private static void Update()
        {
            if (States.Count == 0)
            {
                StopUpdateLoop();
                return;
            }

            if (Application.isPlaying)
            {
                StopAllPreviewSystems();
                return;
            }

            if (!HasActivePreviewStates())
            {
                StopUpdateLoop();
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            float deltaTime = Mathf.Max(0f, (float)(now - _lastGlobalTickTime));
            _lastGlobalTickTime = now;

            List<FxSystem> systems = new(States.Keys);
            for (int i = 0; i < systems.Count; i++)
            {
                FxSystem fxSystem = systems[i];
                if (!fxSystem)
                {
                    States.Remove(fxSystem);
                    continue;
                }

                PreviewState state = States[fxSystem];
                if (state.IsPaused)
                    continue;

                if (!fxSystem.IsPlaying)
                {
                    States.Remove(fxSystem);
                    continue;
                }

                TickPreview(fxSystem, state, deltaTime);
            }

            if (States.Count == 0)
            {
                StopUpdateLoop();
                return;
            }

            DOTween.ManualUpdate(deltaTime, deltaTime);
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        private static void StopAllPreviewSystems()
        {
            if (States.Count == 0)
            {
                StopUpdateLoop();
                return;
            }

            List<FxSystem> systems = new(States.Keys);
            for (int i = 0; i < systems.Count; i++)
                ResetPreview(systems[i]);
        }

        private static bool HasActivePreviewStates()
        {
            foreach (PreviewState state in States.Values)
            {
                if (!state.IsPaused)
                    return true;
            }

            return false;
        }

        private static void TickPreview(FxSystem fxSystem, PreviewState state, float deltaTime)
        {
            state.ElapsedTime += Mathf.Max(0f, deltaTime);
            StartDueEffects(state);

            for (int i = 0; i < state.PlayingEffects.Count; i++)
            {
                PreviewPlayingEffect playingEffect = state.PlayingEffects[i];
                if (playingEffect.Effect.IsPlaying)
                    playingEffect.Effect.Tick(deltaTime);
            }

            for (int i = state.PlayingEffects.Count - 1; i >= 0; i--)
            {
                PreviewPlayingEffect playingEffect = state.PlayingEffects[i];
                if (playingEffect.EndTime > state.ElapsedTime) continue;

                playingEffect.Effect.IsPlaying = false;
                state.PlayingEffects.RemoveAt(i);
            }

            if (state.NextItemIndex < fxSystem.Items.Count || state.PlayingEffects.Count != 0) return;

            ResetPreview(fxSystem);
            fxSystem.finishedPlaying?.Invoke();

            if (fxSystem.LoopForever)
                PlayPreview(fxSystem);
        }

        private static void StartDueEffects(PreviewState state)
        {
            IReadOnlyList<FxItem> items = state.FxSystem.Items;

            while (state.NextItemIndex < items.Count && state.ElapsedTime >= state.NextSequentialStartTime)
            {
                FxItem fxItem = items[state.NextItemIndex++];
                Effect effect = fxItem?.Effect;
                if (effect == null) continue;

                effect.IsPlaying = true;
                PlayEffect(effect);

                bool isLoopingMultiEffect = effect is MultiEffect multiEffect &&
                                            (multiEffect.FxSystem?.LoopForever ?? false);

                if (!isLoopingMultiEffect)
                {
                    float duration = Mathf.Max(0f, effect.Duration);
                    state.PlayingEffects.Add(new PreviewPlayingEffect(effect, state.ElapsedTime + duration));
                }

                if (effect.ShouldPlayAsync) continue;

                if (isLoopingMultiEffect)
                {
                    state.NextSequentialStartTime = state.ElapsedTime;
                    continue;
                }

                float effectDuration = Mathf.Max(0f, effect.Duration);
                state.NextSequentialStartTime = state.ElapsedTime + effectDuration;
                if (effectDuration > 0f)
                    break;
            }
        }

        private static void PlayEffect(Effect effect)
        {
            if (effect is MultiEffect multiEffect && multiEffect.FxSystem != null)
            {
                PlayPreview(multiEffect.FxSystem);
                return;
            }

            effect.Play();
        }

        private sealed class PreviewState
        {
            public PreviewState(FxSystem fxSystem)
            {
                FxSystem = fxSystem;
                NextSequentialStartTime = 0f;
            }

            public FxSystem FxSystem { get; }
            public bool IsPaused { get; set; }
            public int NextItemIndex { get; set; }
            public float ElapsedTime { get; set; }
            public float NextSequentialStartTime { get; set; }
            public List<PreviewPlayingEffect> PlayingEffects { get; } = new();
        }

        private readonly struct PreviewPlayingEffect
        {
            public PreviewPlayingEffect(Effect effect, float endTime)
            {
                Effect = effect;
                EndTime = endTime;
            }

            public Effect Effect { get; }
            public float EndTime { get; }
        }
    }
}
