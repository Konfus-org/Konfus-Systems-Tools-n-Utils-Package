using UnityEngine;

namespace Konfus.Fx_System.Scenes
{
    [RequireComponent(typeof(SceneTransition))]
    public class PlaySceneTransitionOnStart : MonoBehaviour
    {
        private void Start()
        {
            var sceneTransition = GetComponent<SceneTransition>();
            sceneTransition.PlayTransition();
        }
    }
}