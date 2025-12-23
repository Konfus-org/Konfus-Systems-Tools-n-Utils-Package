using Konfus.Utility.Attributes;
using Konfus.Utility.Time;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Konfus.Scenes
{
    [Provide]
    public class SceneManager : MonoBehaviour
    {
        public UnityEvent? loadingScene;
        public UnityEvent? sceneLoaded;
        public UnityEvent? sceneUnloaded;

        [SerializeField]
        [ReadOnly]
        private string currentMainScene = "";

        private Timer? _timer;
        private UnityAction? _transitionIntoSceneAction;

        public string CurrentMainScene => currentMainScene;

        private void Start()
        {
            UnitySceneManager.sceneLoaded += OnSceneLoaded;
            UnitySceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy()
        {
            UnitySceneManager.sceneLoaded -= OnSceneLoaded;
            UnitySceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public void TransitionToScene(
            string sceneToTransitionTo,
            string loadingScreenScene,
            string inTransitionScene,
            string outTransitionScene,
            float minLoadingTimeInSeconds,
            float inTransitionTimeInSeconds,
            float outTransitionTimeInSeconds)
        {
            DontDestroyOnLoad(this);
            PlayTransitionOutOfScene(
                sceneToTransitionTo, loadingScreenScene, inTransitionScene, outTransitionScene, minLoadingTimeInSeconds,
                inTransitionTimeInSeconds, outTransitionTimeInSeconds);
        }

        public void GoToScene(string sceneName)
        {
            currentMainScene = sceneName;
            UnitySceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            loadingScene?.Invoke();
        }

        public void AddScene(string sceneName)
        {
            UnitySceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            loadingScene?.Invoke();
        }

        public void RemoveScene(string sceneName)
        {
            UnitySceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        }

        private void PlayTransitionOutOfScene(
            string sceneToTransitionTo,
            string loadingScreenScene,
            string inTransitionScene,
            string outTransitionScene,
            float minLoadingTimeInSeconds,
            float inTransitionTimeInSeconds,
            float outTransitionTimeInSeconds)
        {
            AddScene(outTransitionScene);
            _timer = new Timer(
                outTransitionTimeInSeconds * 1000,
                onStop: () => OnTransitionOutOfSceneComplete(
                    sceneToTransitionTo, loadingScreenScene, inTransitionScene,
                    minLoadingTimeInSeconds, inTransitionTimeInSeconds));
            _timer.Start();
        }

        private void OnTransitionOutOfSceneComplete(
            string sceneToTransitionTo,
            string loadingScreenScene,
            string inTransitionScene,
            float minLoadingTimeInSeconds,
            float inTransitionTimeInSeconds)
        {
            GoToScene(loadingScreenScene);
            _timer = new Timer(
                minLoadingTimeInSeconds * 1000,
                onStop: () =>
                {
                    _transitionIntoSceneAction = () =>
                        PlayTransitionIntoScene(loadingScreenScene, inTransitionScene, inTransitionTimeInSeconds);
                    sceneLoaded?.AddListener(_transitionIntoSceneAction);
                    AddScene(sceneToTransitionTo);
                    currentMainScene = sceneToTransitionTo;
                });
            _timer.Start();
        }

        private void PlayTransitionIntoScene(string loadingScreenScene, string inTransitionScene,
            float inTransitionTimeInSeconds)
        {
            sceneLoaded?.RemoveListener(_transitionIntoSceneAction);
            RemoveScene(loadingScreenScene);
            AddScene(inTransitionScene);
            _timer = new Timer(
                inTransitionTimeInSeconds * 1000,
                onStop: () => OnTransitionIntoSceneComplete(inTransitionScene));
            _timer.Start();
        }

        private void OnTransitionIntoSceneComplete(string inTransitionScene)
        {
            _transitionIntoSceneAction = null;
            RemoveScene(inTransitionScene);
            Destroy(this);
        }

        private void OnSceneUnloaded(Scene unloadedScene)
        {
            sceneUnloaded?.Invoke();
        }

        private void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            sceneLoaded?.Invoke();
        }
    }
}