using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class AudioEffect : Effect
    {
        [SerializeField, Range(0, 1)]
        private float minVolume;
        [SerializeField, Range(0, 1)]
        private float maxVolume;
        [SerializeField, Range(0, 1)]
        private float minPitch;
        [SerializeField, Range(0, 1)]
        private float maxPitch;
        [SerializeField]
        private AudioClip audioClip;
        [SerializeField]
        private AudioSource audioSource;

        public override float Duration
        {
            get
            {
                if (audioSource == null) return 0;
                return audioSource.clip.length;
            }
        }

        public override void Play()
        {
            audioSource.volume = Random.Range(minVolume, maxVolume);
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(audioClip);
        }

        public override void Stop()
        {
            audioSource.Stop();
        }
    }
}