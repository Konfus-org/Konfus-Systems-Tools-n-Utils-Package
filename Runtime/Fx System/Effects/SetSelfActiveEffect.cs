using System;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class SetSelfActiveEffect : Effect
    {
        [SerializeField]
        private bool active;

        private GameObject? _go;

        public override float Duration => 0;

        public override void Initialize(GameObject parentGo)
        {
            _go = parentGo;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            if (!_go)
            {
                Debug.LogWarning($"{nameof(SetGameObjectActiveEffect)} requires a game object to be set.");
                return;
            }

            _go.SetActive(active);
        }

        public override void Stop()
        {
        }
    }
}