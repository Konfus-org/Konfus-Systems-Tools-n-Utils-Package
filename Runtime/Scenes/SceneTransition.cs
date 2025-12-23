using Konfus.Fx_System;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Scenes
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The transition effect on entering a scene.")]
        private FxSystem? transitionFx;

        [Space]
        public UnityEvent? transitionComplete;

        public void PlayTransition()
        {
            if (!transitionFx)
            {
                Debug.LogWarning("Transition FX is not set.");
                return;
            }

            transitionFx.PlayEffects();
            transitionFx.finishedPlaying?.AddListener(OnFinishSceneTransition);
        }

        private void OnFinishSceneTransition()
        {
            transitionComplete?.Invoke();
        }
    }
}