using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class InstantiateEffect : Effect
    {
        [SerializeField]
        private GameObject gameObject;

        private Transform _transformToSpawnAt;

        public override float Duration => 0;

        public override void Initialize(GameObject parentGo)
        {
            _transformToSpawnAt = parentGo.transform;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            Object.Instantiate(gameObject, _transformToSpawnAt.position, Quaternion.identity);
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}