using System;
using Konfus.Systems.Fx_System.Effects;
using UnityEngine;

namespace Armored_Felines.Effects
{
    [Serializable]
    public class SetSelfActiveEffect : Effect
    {
        [SerializeField] private bool active;
        
        private GameObject _go;

        public override float Duration => 0;
        
        public override void Initialize(GameObject parentGo)
        {
            _go = parentGo;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            _go.SetActive(active);
        }

        public override void Stop()
        {
        }
    }
}
