using System;
using Konfus.Systems.Fx_System.Effects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Armored_Felines.Effects
{
    [Serializable]
    public class InstantiateAndRotateToTarget : Effect
    {
        [SerializeField]
        private Transform target;
        [SerializeField]
        private GameObject gameObject;

        [SerializeField]
        private Vector3 rotation;

        public override float Duration => 0;

        public override void Play()
        {
            Object.Instantiate(gameObject, target.position, Quaternion.Euler(target.rotation.eulerAngles + rotation));
        }

        public override void Stop()
        {
        }
    }
}
