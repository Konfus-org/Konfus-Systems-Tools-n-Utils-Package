using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Konfus.Editor.Context_Menu;
using Konfus.Editor.Utility;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Konfus.Editor.Automation
{
    [InitializeOnLoad]
    internal static class ProjectAutoSetup
    {
        private static ListRequest? _request;
        private static readonly string[] _requiredPackageIds =
        {
            "com.unity.services.multiplayer",
            "com.unity.multiplayer.center",
            "com.unity.multiplayer.tools",
            "com.unity.netcode.gameobjects"
        };
        private static AddRequest? _addRequest;
        private static bool _isEnsuring;

        static ProjectAutoSetup()
        {
            ConfirmDialog.Show(
                "Auto Setup?",
                "This will attempt to auto add required packages for the Konfus plugin. " +
                "It will also auto setup a folder structure for the project and generate some code for menu items. Continue?",
                result =>
                {
                    if (!result) return;
                    Setup();
                }
            );
        }

        public static bool SetupIsRunning { get; private set; }

        [MenuItem("Tools/Konfus/Setup New Project", priority = 1000)]
        private static void Setup()
        {
            SetupIsRunning = true;

            // Setup folder structure and generate menu items
            ProjectManager.SetupFolderStructure();
            PinnedMenuItems.GenerateContextMenuItems(false);
            ScriptTemplates.GenerateContextMenuItems(false);

            // Request the list of packages installed in the project so we can ensure we have all the required deps
            _request = Client.List();
            EditorApplication.update += GetRequiredPackages;
        }

        [MenuItem("Tools/Konfus/Update", priority = 1000)]
        private static void Update()
        {
            // Update package
            InstallViaUpm("Konfus Tools n' Utils",
                "https://github.com/Konfus-org/Konfus-Systems-Tools-n-Utils-Package.git");

            // Request the list of packages installed in the project so we can ensure we have all the required deps
            _request = Client.List();
            EditorApplication.update += GetRequiredPackages;
        }

        private static void GetRequiredPackages()
        {
            if (_request is not { IsCompleted: true }) return;

            EditorApplication.update -= GetRequiredPackages;
            var missingDeps = new List<string>();
            switch (_request.Status)
            {
                case StatusCode.Success:
                {
                    foreach (string requiredPackageId in _requiredPackageIds)
                    {
                        if (_request.Result.Any(package => package.name == requiredPackageId)) continue;
                        missingDeps.Add(requiredPackageId);
                        Debug.LogError(
                            $"Required package '{requiredPackageId}' is missing!");
                    }

                    break;
                }
                case >= StatusCode.Failure:
                {
                    Debug.LogError("Could not retrieve package list: " + _request.Error.message);
                    break;
                }
            }

            bool isOdinPresent = IsOdinPresent();
            bool isDotweenPresent = IsDotweenPresent();
            if (!missingDeps.Any() && isOdinPresent && isDotweenPresent) return;

            Debug.LogWarning("Required SDKs are not installed!");
            var msg = "The following required dependencies are not installed:";
            if (!isOdinPresent) msg += "\n-Odin Inspector";
            if (!isDotweenPresent) msg += "\n-DOTween";
            msg = missingDeps.Aggregate(msg, (current, dep) => current + $"\n-{dep}");

            bool install = EditorUtility.DisplayDialog(
                "Required SDKs missing!",
                $"{msg}",
                "Install",
                "Dismiss");
            if (!install) return;

            foreach (string requiredPackageId in _requiredPackageIds)
            {
                InstallViaUpm(requiredPackageId, requiredPackageId);
            }

            if (!isOdinPresent)
            {
                Application.OpenURL(
                    "https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041");
            }

            if (!isDotweenPresent)
            {
                Application.OpenURL(
                    "https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416");
            }

            SetupIsRunning = false;
        }

        private static bool IsOdinPresent()
        {
            return TypeExists("Sirenix.OdinInspector.OdinInspectorConfig")
                   || TypeExists("Sirenix.Serialization.SerializationUtility");
        }

        private static bool IsDotweenPresent()
        {
            return TypeExists("DG.Tweening.DOTween")
                   || TypeExists("DG.Tweening.Tween");
        }

        private static bool TypeExists(string fullTypeName)
        {
            // Fast-ish: scan loaded assemblies once per call
            foreach (Assembly? asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? t = asm.GetType(fullTypeName, false);
                if (t != null) return true;
            }

            return false;
        }

        private static void InstallViaUpm(string displayName, string upmIDOrGit)
        {
            try
            {
                _addRequest = Client.Add(upmIDOrGit);
                EditorApplication.update += OnUpmAddProgress;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to start UPM install for {displayName}: {e}");
            }
        }

        private static void OnUpmAddProgress()
        {
            if (_addRequest == null) return;
            if (!_addRequest.IsCompleted) return;

            EditorApplication.update -= OnUpmAddProgress;

            if (_addRequest.Status == StatusCode.Success)
                Debug.Log($"UPM installed: {_addRequest.Result.name} {_addRequest.Result.version}");
            else
                Debug.LogError($"UPM install failed: {_addRequest.Error?.message}");

            _addRequest = null;
        }
    }
}