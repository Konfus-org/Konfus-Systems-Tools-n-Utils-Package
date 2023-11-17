using System;
using System.Collections;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Konfus.Systems.FX
{
    public class FxSystem : SerializedMonoBehaviour
    {
        [OdinSerialize]
        private IEffect[] effects;

        public void PlayEffects()
        {
            StartCoroutine(PlayEffectsCoroutine());
        }

        private IEnumerator PlayEffectsCoroutine()
        {
            foreach (IEffect effect in effects)
            {
                effect.Play();
                float playTime = effect.GetPlayTime();
                yield return new WaitForSeconds(playTime);
            }

            yield return null;
        }

        private void Start()
        {
            foreach (IEffect effect in effects)
            {
                effect.Initialize(gameObject);
            }
        }
    }
}