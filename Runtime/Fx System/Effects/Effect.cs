using System;
using UnityEngine;

namespace Konfus.Fx_System.Effects
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

        /// <summary>
        /// Called in the Awake method of the owning FxSystem, used for any init logic for the effect, c
        /// </summary>
        public virtual void Initialize(GameObject parentGo) { }
        
        /// <summary>
        /// Called when the owning FxSystem is played, the meat of most effects will be go here
        /// </summary>
        public abstract void Play();
        
        /// <summary>
        /// Called when the owning FxSystem is stopped, used for any cleanup or stop logic
        /// </summary>
        public abstract void Stop();
        public virtual void OnValidate() { }
    }
}