using System;
using Konfus.Utility.Attributes;
using Konfus.Utility.Time;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Armored_Felines.Scenes
{
    [Provide]
    public class SceneManager : MonoBehaviour
    {
        public UnityEvent loadingScene;
        public UnityEvent sceneLoaded;
        public UnityEvent sceneUnloaded;
        
        [SerializeField, ReadOnly]
        private string currentMainScene;
        
        public string CurrentMainScene => currentMainScene;
        
        private Timer _timer;
        private UnityAction _transitionIntoSceneAction;
        
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
                sceneToTransitionTo, loadingScreenScene, inTransitionScene, outTransitionScene, minLoadingTimeInSeconds, inTransitionTimeInSeconds, outTransitionTimeInSeconds);
        }

        public void GoToScene(string sceneName)
        {
            currentMainScene = sceneName;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            loadingScene.Invoke();
        }
        
        public void AddScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            loadingScene.Invoke();
        }
        
        public void RemoveScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        }

        private void Start()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
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
                durationInMilliseconds: outTransitionTimeInSeconds * 1000,
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
                durationInMilliseconds: minLoadingTimeInSeconds * 1000,
                onStop: () =>
                {
                    _transitionIntoSceneAction = () =>
                        PlayTransitionIntoScene(loadingScreenScene, inTransitionScene, inTransitionTimeInSeconds);
                    sceneLoaded.AddListener(_transitionIntoSceneAction);
                    AddScene(sceneToTransitionTo);
                    currentMainScene = sceneToTransitionTo;
                });
            _timer.Start();
        }

        private void PlayTransitionIntoScene(string loadingScreenScene, string inTransitionScene, float inTransitionTimeInSeconds)
        {
            sceneLoaded.RemoveListener(_transitionIntoSceneAction);
            RemoveScene(loadingScreenScene);
            AddScene(inTransitionScene);
            _timer = new Timer(
                durationInMilliseconds: inTransitionTimeInSeconds * 1000,
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
            sceneUnloaded.Invoke();
        }
        
        private void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            sceneLoaded.Invoke();
        }
    }
}
