using System;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class SetGameObjectActiveEffect : Effect
    {
        [SerializeField]
        private bool isGameObjectActive;

        [SerializeField]
        private GameObject? gameObject;

        public override float Duration => 0;

        public override void Play()
        {
            if (!gameObject)
            {
                Debug.LogWarning($"{nameof(SetGameObjectActiveEffect)} requires a game object to be set.");
                return;
            }

            gameObject.SetActive(isGameObjectActive);
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}