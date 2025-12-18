using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class DestroyEffect : Effect
    {
        [SerializeField]
        private GameObject gameObjectToDestroy;

        public override float Duration => 0;

        public override void Play()
        {
            Object.Destroy(gameObjectToDestroy);
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}