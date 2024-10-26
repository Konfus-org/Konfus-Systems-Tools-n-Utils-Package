using System;
using UnityEngine;

namespace Konfus.Systems.Fx_System.Effects
{
    [Serializable]
    public class SetGameObjectActiveEffect : Effect
    {
        [SerializeField]
        private bool isGameObjectActive;
        [SerializeField]
        private GameObject gameObject;

        public override float Duration => 0;

        public override void Play()
        {
            gameObject.SetActive(isGameObjectActive);
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}