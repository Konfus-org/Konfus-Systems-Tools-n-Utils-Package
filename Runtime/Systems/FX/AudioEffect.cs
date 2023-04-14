using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class AudioEffect : Effect
    {
        [SerializeField]
        private AudioClip audioClip;

        public override void Play()
        {
            // TODO: implement
        }
    }
}