using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class SetActiveEffect : Effect
    {
        [SerializeField]
        private bool active;
        private GameObject gameObject;
        
        public override void Initialize(GameObject parentGo)
        {
            gameObject = parentGo;
        }

        public override void Play()
        {
            gameObject.SetActive(active);
        }
    }
}