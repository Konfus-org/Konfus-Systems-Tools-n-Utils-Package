using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Systems.Fx_System.Effects
{
    [Serializable]
    public class DestroySelfEffect : Effect
    {
        private GameObject _gameObjectToDestroy;

        public override float Duration => 0;

        public override void Initialize(GameObject parentGo)
        {
            _gameObjectToDestroy = parentGo;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            Object.Destroy(_gameObjectToDestroy);
        }

        public override void Stop()
        {
            // Do nothing
        }
    }
}