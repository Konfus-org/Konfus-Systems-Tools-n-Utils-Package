using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Systems.FX
{
    // TODO: create editor script for this!!!
    public class FxSystem : MonoBehaviour
    {
        [SerializeField]
        private List<Effect> effects;

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