using System;
using UnityEngine;

namespace Konfus.Systems.Fx_System.Effects
{
    [Serializable]
    public abstract class Effect
    {
        [SerializeField, Tooltip("If true, the owning system won't wait for this effect to finish before playing the next effect.")]
        private bool playAsync;
        
        /// <summary>
        /// If true the owning <see cref="FxSystem"/> won't wait for this to be finished playing to play the next effect.
        /// </summary>
        public bool ShouldPlayAsync => playAsync;
        
        /// <summary>
        /// Whether or not the effect is playing
        /// </summary>
        public bool IsPlaying
        {
            get => _isPlaying;
            internal set => _isPlaying = value;
        }
        private bool _isPlaying;
        
        /// <summary>
        /// Effect duration in seconds.
        /// </summary>
        public abstract float Duration { get; }

        public virtual void Initialize(GameObject parentGo) { }
        public abstract void Play();
        public abstract void Stop();
    }
}