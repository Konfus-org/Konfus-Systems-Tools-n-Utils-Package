using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public abstract class Effect : IEffect
    {
        [SerializeField, Min(0)]
        private float playTimeInSeconds;

        [SerializeField] 
        private Effect value;
        
        public float GetPlayTime() => playTimeInSeconds;
        public virtual void Initialize(GameObject parentGo) { }
        public abstract void Play();
    }
}