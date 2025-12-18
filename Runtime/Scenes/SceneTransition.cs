using Konfus.Fx_System;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Fx_System.Scenes
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField, Tooltip("The transition effect on entering a scene.")]
        private FxSystem transitionFx;
        [Space]
        public UnityEvent transitionComplete;

        public void PlayTransition()
        {
            transitionFx.PlayEffects();
            transitionFx.finishedPlaying.AddListener(OnFinishSceneTransition);
        }

        private void OnFinishSceneTransition()
        {
            transitionComplete.Invoke();
        }
    }
}