using System;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public abstract class Effect
    {
        [SerializeField]
        [Tooltip("If true, the owning system won't wait for this effect to finish before playing the next effect.")]
        private bool playAsync;

        private bool _isPlaying;

        /// <summary>
        /// If true the owning <see cref="FxSystem" /> won't wait for this to be finished playing to play the next effect.
        /// </summary>
        public bool ShouldPlayAsync => playAsync;

        /// <summary>
        /// Whether the effect is playing
        /// </summary>
        public bool IsPlaying
        {
            get => _isPlaying;
            set => _isPlaying = value;
        }

        /// <summary>
        /// Effect duration in seconds.
        /// </summary>
        public abstract float Duration { get; }

        /// <summary>
        /// Called in the Awake method of the owning FxSystem, used for any init logic for the effect, c
        /// </summary>
        public virtual void Initialize(GameObject parentGo)
        {
        }

        /// <summary>
        /// Called when the owning FxSystem is played, the meat of most effects will go here
        /// </summary>
        public abstract void Play();

        /// <summary>
        /// Called when the owning FxSystem is paused, used for non-destructive pause logic
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// Called when a paused FxSystem resumes.
        /// </summary>
        public virtual void Resume()
        {
        }

        /// <summary>
        /// Called when the owning FxSystem is explicitly reset.
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// Called when the owning FxSystem is fully stopped (pause + reset).
        /// </summary>
        public virtual void Stop()
        {
            Pause();
            Reset();
        }

        /// <summary>
        /// Called each preview tick while this effect is playing in edit mode.
        /// </summary>
        public virtual void Tick(float deltaTime)
        {
        }

        public virtual void OnValidate()
        {
        }
    }
}
