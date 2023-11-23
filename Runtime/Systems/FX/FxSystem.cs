using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Konfus.Systems.FX
{
    public class FxSystem : SerializedMonoBehaviour
    {
        [OdinSerialize]
        private List<IEffect> effects = new List<IEffect>();

        private bool _isPlaying;

        public void PlayEffects()
        {
            if (_isPlaying) return;
            StartCoroutine(PlayEffectsCoroutine());
        }

        private IEnumerator PlayEffectsCoroutine()
        {
            _isPlaying = true;
            
            foreach (IEffect effect in effects)
            {
                effect.Play();
                float playTime = effect.GetPlayTime();
                yield return new WaitForSeconds(playTime);
            }
            
            _isPlaying = false;
            yield return null;
        }

        private void OnDisable()
        {
            _isPlaying = false;
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