using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class ParticleEffect : Effect
    {
        [SerializeField]
        private ParticleSystem particleSystem;

        public override void Play()
        {
            particleSystem.Play();
        }

        public override void Stop()
        {
            particleSystem.Stop();
        }
    }
}