using Konfus.Utility.Attributes;
using UnityEngine;

namespace Konfus.Scenes
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField]
        [ScenePicker]
        [Tooltip("Scene to load into")]
        private string sceneToLoad = "";

        [SerializeField]
        [ScenePicker]
        [Tooltip("Loading screen scene")]
        private string loadingScene = "";

        [SerializeField]
        [ScenePicker]
        [Tooltip("The scene to load that contains transition out of scene effects")]
        private string outTransitionScene = "";

        [SerializeField]
        [ScenePicker]
        [Tooltip("The scene to load that contains transition into scene effects")]
        private string inTransitionScene = "";

        [SerializeField]
        [Range(0, 10)]
        [Tooltip("Time to play the load out of scene transition effects")]
        private float outTransitionTimeInSeconds = 3;

        [SerializeField]
        [Range(0, 10)]
        [Tooltip("Time to play the load into scene transition effects")]
        private float inTransitionTimeInSeconds = 3;

        [SerializeField]
        [Range(0, 10)]
        [Tooltip("Minimum time to show loading scene")]
        private float minLoadingTimeInSeconds = 3;

        [Inject]
        private SceneManager? _sceneManager;

        public void LoadScene()
        {
            _sceneManager?.TransitionToScene(
                sceneToLoad, loadingScene, inTransitionScene, outTransitionScene, minLoadingTimeInSeconds,
                inTransitionTimeInSeconds, outTransitionTimeInSeconds);
        }
    }
}