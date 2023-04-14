using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class DestroyEffect : Effect
    {
        [SerializeField]
        private GameObject gameObjectToDestroy;

        public override void Play()
        {
            Object.Destroy(gameObjectToDestroy);
        }
    }
}