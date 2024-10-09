using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public abstract class Effect : IEffect
    {
        [SerializeField, Range(0f, 60f), Tooltip("The time to play the effect in seconds")]
        private float playTimeInSeconds;
        
        public float GetPlayTime() => playTimeInSeconds;
        public virtual void Initialize(GameObject parentGo) { }
        public abstract void Play();
    }
}