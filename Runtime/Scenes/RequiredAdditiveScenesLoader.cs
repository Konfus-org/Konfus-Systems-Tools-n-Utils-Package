using System;
using Konfus.Utility.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Konfus.Scenes
{
    public class RequiredAdditiveScenesLoader : MonoBehaviour
    {
        [Tooltip(
            "Required scenes to load additively, if you don't see your scene in the list make sure its been added to the scene list in build settings!")]
        [SerializeField]
        [ScenePicker]
        private string[]? requiredAdditiveScenes;

        private void Awake()
        {
            foreach (string requiredAdditiveScene in requiredAdditiveScenes ?? Array.Empty<string>())
            {
                UnitySceneManager.LoadScene(requiredAdditiveScene, LoadSceneMode.Additive);
            }
        }
    }
}