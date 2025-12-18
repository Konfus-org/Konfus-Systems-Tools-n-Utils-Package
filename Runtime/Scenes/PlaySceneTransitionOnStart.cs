using UnityEngine;

namespace Armored_Felines.Scenes
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