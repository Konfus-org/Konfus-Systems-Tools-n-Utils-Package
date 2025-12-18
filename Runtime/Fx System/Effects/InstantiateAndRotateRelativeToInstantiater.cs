using System;
using Konfus.Systems.Fx_System.Effects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Armored_Felines.Effects
{
    [Serializable]
    public class InstantiateAndRotateRelativeToInstantiater : Effect
    {
        [SerializeField]
        private GameObject gameObject;

        [SerializeField]
        private Vector3 rotation;
        
        private Transform _transformToSpawnAt;

        public override float Duration => 0;

        public override void Initialize(GameObject parentGo)
        {
            _transformToSpawnAt = parentGo.transform;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            Object.Instantiate(gameObject, _transformToSpawnAt.position, Quaternion.Euler(_transformToSpawnAt.rotation.eulerAngles + rotation));
        }

        public override void Stop()
        {
        }
    }
}
