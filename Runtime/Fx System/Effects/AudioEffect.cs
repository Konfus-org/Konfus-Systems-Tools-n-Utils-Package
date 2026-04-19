using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class AudioEffect : Effect
    {
        [SerializeField]
        [Range(0, 1)]
        private float minVolume;

        [SerializeField]
        [Range(0, 1)]
        private float maxVolume;

        [SerializeField]
        [Range(0, 1)]
        private float minPitch;

        [SerializeField]
        [Range(0, 1)]
        private float maxPitch;

        [SerializeField]
        private AudioClip? audioClip;

        [SerializeField]
        private AudioSource? audioSource;

        public override float Duration
        {
            get
            {
                if (audioClip != null) return audioClip.length;
                if (audioSource?.clip != null) return audioSource.clip.length;
                return 0f;
            }
        }

        public override void Play()
        {
            if (!audioSource)
            {
                Debug.LogWarning("AudioSource is not set");
                return;
            }

            audioSource.volume = Random.Range(minVolume, maxVolume);
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(audioClip);
        }

        public override void Pause()
        {
            if (!audioSource) return;
            audioSource.Pause();
        }

        public override void Resume()
        {
            if (!audioSource) return;
            audioSource.UnPause();
        }

        public override void Reset()
        {
            if (!audioSource) return;
            audioSource.Stop();
        }
    }
}
