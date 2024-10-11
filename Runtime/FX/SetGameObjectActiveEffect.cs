using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class SetGameObjectActiveEffect : Effect
    {
        [SerializeField]
        private bool active;
        [SerializeField]
        private GameObject gameObject;

        public override void Play()
        {
            gameObject.SetActive(active);
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}