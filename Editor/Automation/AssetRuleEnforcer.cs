using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Konfus.Editor.Utility;
using Konfus.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Automation
{
    internal static class AssetRuleEnforcer
    {
        private static bool _isProcessing;

        [MenuItem("Tools/Konfus/Assets/Auto Organize Assets", priority = 0)]
        private static void OrganizeAssets()
        {
            _isProcessing = true;
            RunOnAssets(false, true, true);
            _isProcessing = false;
        }

        [MenuItem("Tools/Konfus/Assets/Auto Fix Assets Naming", priority = 0)]
        private static void SuffixAssets()
        {
            _isProcessing = true;
            RunOnAssets(true, false, true);
            _isProcessing = false;
        }

        [MenuItem("Assets/Rules/Auto Organize Selection", priority = 0)]
        private static void OrganizeSelection()
        {
            _isProcessing = true;
            RunOnSelection(false, true, true);
            _isProcessing = false;
        }

        [MenuItem("Assets/Rules/Fix Selections Naming", priority = 0)]
        private static void SuffixSelection()
        {
            _isProcessing = true;
            RunOnSelection(true, false, true);
            _isProcessing = false;
        }

        private static void RunOnAssets(bool rename, bool move, bool manuallyTriggered = false)
        {
            string[] paths = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith(ProjectManager.RootPath, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Dictionary<string, string> suggestions =
                ProjectManager.SuggestChanges(paths, rename, move, manuallyTriggered);
            Run(suggestions, manuallyTriggered);
        }

        private static void RunOnSelection(bool rename, bool move, bool manuallyTriggered = false)
        {
            string[] selectedPaths = Selection.objects
                .Select(AssetDatabase.GetAssetPath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();

            var expanded = new List<string>();
            foreach (string p in selectedPaths)
            {
                if (AssetDatabase.IsValidFolder(p))
                {
                    expanded.AddRange(AssetDatabase.FindAssets("", new[] { p })
                        .Select(AssetDatabase.GUIDToAssetPath));
                }
                else
                    expanded.Add(p);
            }

            Dictionary<string, string> suggestions =
                ProjectManager.SuggestChanges(expanded, rename, move, manuallyTriggered);
            Run(suggestions, manuallyTriggered);
        }

        private static void Run(Dictionary<string, string> changeToMake, bool manuallyTriggered = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var updated = 0;
            var warnings = new List<string>();

            using (new AssetDatabase.AssetEditingScope())
            {
                foreach ((string _, string newPath) in changeToMake)
                {
                    ProjectManager.Ensure(newPath);
                }
            }

            AssetDatabase.Refresh();

            using (new AssetDatabase.AssetEditingScope())
            {
                foreach ((string oldPath, string newPath) in changeToMake)
                {
                    if (ProjectManager.TryMove(oldPath, Path.GetDirectoryName(newPath) ?? string.Empty,
                            out string? warn))
                        updated++;
                    if (warn != null)
                        warnings.Add(warn);
                }
            }

            AssetDatabase.Refresh();

            string warningMsg = warnings.IsNullOrEmpty() ? string.Join("\n", warnings) : "NONE";
            string changes = changeToMake.Count == 0
                ? "NONE"
                : string.Join("\n", changeToMake.Select(kvp => $"{kvp.Key} -> {kvp.Value}"));
            string userMsg =
                $"Updated: {updated}\n" +
                $"Changes:\n{changes}\n" +
                $"Warnings:\n{warningMsg}";
            bool hasAnyChanges = changeToMake.Count > 0;
            if ((manuallyTriggered && hasAnyChanges) || (!manuallyTriggered && hasAnyChanges && updated > 0))
                EditorUtility.DisplayDialog("Konfus Importer", userMsg, "OK");
            Debug.Log($"Updated: {updated}\n{userMsg}");
        }

        internal sealed class ImportHook : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(
                string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                if (_isProcessing || ProjectAutoSetup.SetupIsRunning) return;
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (importedAssets.IsNullOrEmpty()) return;

                string[] candidates = importedAssets
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToArray();

                if (candidates.Length == 0)
                    return;

                // Build suggestions. If empty: no prompt, no run.
                Dictionary<string, string> suggested = ProjectManager.SuggestChanges(
                    candidates,
                    true,
                    true);

                if (suggested.Count == 0)
                    return;

                ConfirmDialog.Show(
                    "Auto enforce naming conventions and organize?",
                    "\n\nSuggested Changes:\n" +
                    string.Join("\n",
                        suggested.Select(kv =>
                            "   -" + kv.Key + " -> " + kv.Value)) +
                    "\n\nContinue?",
                    result =>
                    {
                        if (!result)
                            return;

                        EditorApplication.delayCall += () =>
                        {
                            _isProcessing = true;
                            try
                            {
                                Run(suggested, true);
                            }
                            finally
                            {
                                _isProcessing = false;
                            }
                        };
                    }
                );
            }
        }
    }
}