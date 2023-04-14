using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public abstract class Effect : IEffect
    {
        [SerializeField]
        private float playTimeInSeconds;
        
        public float GetPlayTime() => playTimeInSeconds;
        public virtual void Initialize(GameObject parentGo) { }
        public abstract void Play();
    }
}