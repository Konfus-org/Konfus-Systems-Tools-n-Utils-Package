using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class EnableDisableComponentEffect : Effect
    {
        [SerializeField]
        private bool enabled;
        [SerializeField]
        private MonoBehaviour component;

        public override void Play()
        {
            component.enabled = enabled;
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}