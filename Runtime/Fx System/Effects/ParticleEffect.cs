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

            particleSystem.Play();
        }

        public override void Stop()
        {
            particleSystem?.Stop();
        }
    }
}