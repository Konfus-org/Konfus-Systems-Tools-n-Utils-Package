using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class InstantiateEffect : Effect
    {
        [SerializeField]
        private GameObject gameObject;

        private Transform _transformToSpawnAt;
        
        public override void Initialize(GameObject parentGo)
        {
            _transformToSpawnAt = parentGo.transform;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            Object.Instantiate(gameObject, _transformToSpawnAt.position, Quaternion.identity);
        }
    }
}