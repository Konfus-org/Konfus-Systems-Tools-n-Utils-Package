using System;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class ParticleEffect : Effect
    {
        [SerializeField]
        private ParticleSystem? particleSystem;

        public override float Duration => particleSystem?.main.duration ?? 0;

        public override void Play()
        {
            if (!particleSystem)
            {
                Debug.LogWarning($"{nameof(ParticleSystem)} requires a ParticleSystem");
                return;
            }

            particleSystem.Play(true);
        }

        public override void Pause()
        {
            if (!particleSystem) return;
            particleSystem.Pause(true);
        }

        public override void Resume()
        {
            if (!particleSystem) return;
            particleSystem.Play(true);
        }

        public override void Reset()
        {
            if (!particleSystem) return;
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Simulate(0f, true, true, true);
        }

        public override void Tick(float deltaTime)
        {
            if (Application.isPlaying || !particleSystem || deltaTime <= 0f) return;
            particleSystem.Simulate(deltaTime, true, false, true);
        }
    }
}
