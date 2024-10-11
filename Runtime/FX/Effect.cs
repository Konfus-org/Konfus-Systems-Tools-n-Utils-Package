using System;
using Konfus.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Konfus.Systems.FX
{
    [Serializable]
    public abstract class Effect
    {
        [SerializeField, ReadOnly]
        private bool isPlaying;
        [SerializeField, Range(0f, 60f), Tooltip("The time to play the effect in seconds before playing the next effect.")]
        private float effectDuration;
        
        public float Duration => effectDuration;
        public bool IsPlaying
        {
            get => isPlaying;
            internal set => isPlaying = value;
        }
        
        public virtual void Initialize(GameObject parentGo) { }
        public abstract void Play();
        public abstract void Stop();
    }
}