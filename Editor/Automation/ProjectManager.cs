using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Konfus.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Automation
{
    public static class ProjectManager
    {
        // Source files (go to Art/Source)
        private static readonly HashSet<string> _sourceExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".psd", ".psb",
            ".blend",
            ".ma", ".mb",
            ".max",
            ".c4d",
            ".kra",
            ".xcf",
            ".spp", ".spsm",
            ".sbs", ".sbsar",
            ".ai",
            ".clip",
            ".ztl"
        };

        private static readonly HashSet<string> _modelExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".fbx", ".obj", ".dae", ".gltf", ".glb"
        };

        private static readonly HashSet<string> _textureExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".tga", ".tif", ".tiff", ".exr", ".hdr"
        };

        private static readonly HashSet<string> _audioExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".wav", ".ogg", ".mp3", ".aiff", ".aif"
        };

        private static readonly HashSet<string> _fontExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".ttf", ".otf"
        };

        private static readonly HashSet<string> _shaderExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".shader", ".hlsl", ".cginc", ".shadergraph", ".shadersubgraph"
        };

        public static readonly string[] FoldersToAutoCreate =
        {
            // Root
            RootPath,

            // Art
            RootPath + "/Art/Source/",
            RootPath + "/Art/Models/",
            RootPath + "/Art/Textures/",
            RootPath + "/Art/Fonts/",
            RootPath + "/Art/Sprites/",
            RootPath + "/Art/Audio/",

            // Prefabs
            RootPath + "/Prefabs/",

            // Rendering
            RootPath + "/Rendering/Shaders/",
            RootPath + "/Rendering/Materials/",

            // Code
            RootPath + "/Code/Runtime/",
            RootPath + "/Code/Editor/",

            // Scenes
            RootPath + "/Scenes/Release/",
            RootPath + "/Scenes/Dev/"
        };

        public static string RootPath => "Assets/_Source/";

        // Art
        public static string SourcePath => RootPath + "/Art/Source/";
        public static string ModelsPath => RootPath + "/Art/Models/";
        public static string TexturesPath => RootPath + "/Art/Textures/";
        public static string FontsPath => RootPath + "/Art/Fonts/";
        public static string SpritesPath => RootPath + "/Art/Sprites/";
        public static string AudioPath => RootPath + "/Art/Audio/";

        // Prefabs
        public static string PrefabsPath => RootPath + "/Prefabs/";

        // Rendering
        public static string ShadersPath => RootPath + "/Rendering/Shaders/";
        public static string MaterialsPath => RootPath + "/Rendering/Materials/";

        // Code
        public static string CodePath => RootPath + "/Code/";
        public static string RuntimeCodePath => RootPath + "/Code/Runtime/";
        public static string RuntimeGeneratedCodePath => RootPath + "/Code/Editor/Generated/";
        public static string EditorCodePath => RootPath + "/Code/Editor/";
        public static string EditorGeneratedCodePath => RootPath + "/Code/Editor/Generated/";

        // Scenes
        public static string ReleaseScenePath => RootPath + "/Scenes/Release/";
        public static string DevScenePath => RootPath + "/Scenes/Dev/";

        [MenuItem("Tools/Konfus/Assets/Setup Folders", priority = 0)]
        public static void SetupFolderStructure()
        {
            var created = 0;
            var existed = 0;

            try
            {
                EnsureFoldersAndFiles(ref created, ref existed);
                EditorUtility.DisplayDialog("Folder Setup",
                    $"Root: {RootPath}\n\nFolders created: {created}\nFolders already existed: {existed}\n",
                    "OK");
            }
            catch (Exception e)
            {
                Debug.LogError($"Folder setup failed: {e}");
                EditorUtility.DisplayDialog("Folder Setup", $"Failed:\n{e.Message}", "OK");
            }
        }

        public static string Absolute(string assetPath)
        {
            return Path.GetFullPath(
                    Path.Combine(Application.dataPath, "..", assetPath))
                .Replace("\\", "/");
        }

        public static string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "NewFile";

            input = input.Replace("\\", "/");
            input = input.Replace("//", "/");

            var sb = new StringBuilder(input.Length);
            foreach (char c in input.Where(c =>
                         char.IsLetterOrDigit(c) || c == ' ' || c == '/' || c == '.' || c == '_'))
            {
                sb.Append(c);
            }

            if (sb.Length == 0)
                return "NewFile";
            if (char.IsDigit(sb[0]))
                sb.Insert(0, string.Empty);

            return sb.ToString();
        }

        /// <summary>
        /// Ensures the full chain exists (e.g. Assets/_Konfus/Scenes/Main).
        /// Returns true if it created at least one folder, false if everything already existed.
        /// </summary>
        public static bool Ensure(string folderPath)
        {
            folderPath = Path.GetDirectoryName(folderPath) ?? folderPath;
            folderPath = Sanitize(folderPath);

            if (!folderPath.StartsWith("Assets", StringComparison.Ordinal))
                throw new InvalidOperationException($"Folder path must be under Assets. Got: {folderPath}");

            if (AssetDatabase.IsValidFolder(folderPath))
                return false;

            string[] parts = folderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts[0] != "Assets")
                throw new InvalidOperationException($"Invalid folder path: {folderPath}");

            var current = "Assets";
            var createdAny = false;

            for (var i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                    createdAny = true;
                }

                current = next;
            }

            return createdAny;
        }

        public static string? GetFolder(string assetPath, string ext)
        {
            if (_sourceExtensions.Contains(ext))
                return SourcePath;

            if (assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                return IsEditorScript(assetPath)
                    ? EditorCodePath
                    : RuntimeCodePath;
            }

            if (ext.Equals(".unity", StringComparison.OrdinalIgnoreCase))
            {
                if (assetPath.IndexOf("/Release/", StringComparison.OrdinalIgnoreCase) >= 0)
                    return ReleaseScenePath;

                return DevScenePath;
            }

            if (ext.Equals(".prefab", StringComparison.OrdinalIgnoreCase))
                return PrefabsPath;

            if (ext.Equals(".mat", StringComparison.OrdinalIgnoreCase))
                return MaterialsPath;

            if (_shaderExtensions.Contains(ext))
                return ShadersPath;

            if (_modelExtensions.Contains(ext))
                return ModelsPath;

            if (_fontExtensions.Contains(ext))
                return FontsPath;

            if (_audioExtensions.Contains(ext))
                return AudioPath;

            if (_textureExtensions.Contains(ext))
            {
                var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (ti != null && ti.textureType == TextureImporterType.Sprite)
                    return SpritesPath;

                return TexturesPath;
            }

            return null;
        }

        public static string? GetAssetSuffix(string assetPath, string ext)
        {
            if (ext.Equals(".prefab", StringComparison.OrdinalIgnoreCase)) return "_Pre";
            if (ext.Equals(".unity", StringComparison.OrdinalIgnoreCase)) return "_Scn";
            if (ext.Equals(".mat", StringComparison.OrdinalIgnoreCase)) return "_Mat";
            if (_shaderExtensions.Contains(ext)) return "_Shd";
            if (_modelExtensions.Contains(ext)) return "_Mdl";
            if (_fontExtensions.Contains(ext)) return "_Fnt";
            if (_audioExtensions.Contains(ext)) return "_Aud";

            if (!_textureExtensions.Contains(ext))
                return null;

            var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti != null && ti.textureType == TextureImporterType.Sprite)
                return "_Spr";

            return "_Tex";
        }

        public static string GetName(string assetPath, string ext, string? suffix)
        {
            string stem = Path.GetFileNameWithoutExtension(assetPath);
            stem = StripKnownSuffix(stem, assetPath, ext);
            string pascal = stem.ToPascalCase();
            return Sanitize(pascal + suffix);
        }

        public static bool ShouldRename(string assetPath)
        {
            string ext = Path.GetExtension(assetPath);
            if (string.IsNullOrEmpty(ext))
                return false;

            string? suffix = GetAssetSuffix(assetPath, ext);
            if (string.IsNullOrEmpty(suffix))
                return false;

            string desiredStem = GetName(assetPath, ext, suffix);
            string currentStem = Path.GetFileNameWithoutExtension(assetPath);

            // Note: rename uses Ordinal to keep exact casing expectations
            return !string.Equals(currentStem, desiredStem, StringComparison.Ordinal);
        }

        public static bool ShouldMove(string assetPath)
        {
            string ext = Path.GetExtension(assetPath);
            if (string.IsNullOrEmpty(ext))
                return false;

            string? targetFolder = GetFolder(assetPath, ext);
            if (string.IsNullOrEmpty(targetFolder))
                return false;

            string? currentFolder = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            return !string.Equals(currentFolder, targetFolder, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Builds a "suggested changes" map of original asset path -> planned final path.
        /// If rename/move are enabled, the returned path represents the desired end state.
        /// If no changes are needed for an asset, it will not be included.
        /// </summary>
        public static Dictionary<string, string> SuggestChanges(
            IEnumerable<string> assetPaths,
            bool renameSuggestions,
            bool moveSuggestions,
            bool disableIgnoreLogic = false)
        {
            var changes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string assetPath in assetPaths
                         .Where(p => !string.IsNullOrEmpty(p))
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!disableIgnoreLogic && ShouldSuggestedChangesIgnore(assetPath))
                    continue;

                string ext = Path.GetExtension(assetPath);
                if (string.IsNullOrEmpty(ext))
                    continue;

                string? targetFolder = GetFolder(assetPath, ext);
                if (string.IsNullOrEmpty(targetFolder))
                    continue;

                string? folder = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
                string fileName = Path.GetFileName(assetPath);

                // Plan rename (stem only, keep extension)
                string plannedFileName = fileName;
                if (renameSuggestions)
                {
                    string? suffix = GetAssetSuffix(assetPath, ext);
                    if (!string.IsNullOrEmpty(suffix))
                    {
                        string desiredStem = GetName(assetPath, ext, suffix);
                        string currentStem = Path.GetFileNameWithoutExtension(assetPath);

                        if (!string.Equals(currentStem, desiredStem, StringComparison.Ordinal))
                            plannedFileName = desiredStem + ext;
                    }
                }

                // Plan move (folder)
                string? plannedFolder = folder;
                if (moveSuggestions)
                {
                    if (!string.Equals(folder, targetFolder, StringComparison.OrdinalIgnoreCase))
                        plannedFolder = targetFolder;
                }

                // If nothing changes, skip
                bool wouldRename = !string.Equals(plannedFileName, fileName, StringComparison.Ordinal);
                bool wouldMove = !string.Equals(plannedFolder, folder, StringComparison.OrdinalIgnoreCase);

                if (!wouldRename && !wouldMove)
                    continue;

                string plannedPath = $"{plannedFolder}/{plannedFileName}".Replace("\\", "/");
                changes[Sanitize(assetPath)] = Sanitize(plannedPath);
            }

            return changes;
        }

        public static bool IsEditorScript(string scriptPath)
        {
            if (scriptPath.Contains("Editor", StringComparison.OrdinalIgnoreCase))
                return true;

            string text = File.ReadAllText(scriptPath);
            if (text.Contains("namespace") && text.Contains(".Editor"))
                return true;

            return text.Contains("using UnityEditor");
        }

        public static string GetUniquePath(string desiredPath)
        {
            desiredPath = desiredPath.Replace("\\", "/");
            if (!File.Exists(desiredPath))
                return desiredPath;

            string folder = Path.GetDirectoryName(desiredPath)!.Replace("\\", "/");
            string stem = Path.GetFileNameWithoutExtension(desiredPath);
            string ext = Path.GetExtension(desiredPath);

            for (var i = 1; i < 1000; i++)
            {
                var candidate = $"{folder}/{stem}_{i:00}{ext}";
                if (!File.Exists(candidate))
                    return candidate;
            }

            return desiredPath;
        }

        public static bool TryRename(string assetPath, string suffix, out string? warning)
        {
            warning = null;

            string folder = Path.GetDirectoryName(assetPath)!.Replace("\\", "/");
            string ext = Path.GetExtension(assetPath);

            string desiredStem = GetName(assetPath, ext, suffix);
            if (string.Equals(Path.GetFileNameWithoutExtension(assetPath), desiredStem, StringComparison.Ordinal))
                return false;

            string targetPath = GetUniquePath($"{folder}/{desiredStem}{ext}");
            string targetNameNoExt = Path.GetFileNameWithoutExtension(targetPath);

            string renameError = AssetDatabase.RenameAsset(assetPath, targetNameNoExt);
            if (!string.IsNullOrEmpty(renameError))
            {
                warning = $"Rename failed:\n{assetPath}\nError: {renameError}";
                return false;
            }

            return true;
        }

        public static bool TryMove(string assetPath, string targetFolder, out string? warning)
        {
            warning = null;

            if (string.IsNullOrEmpty(targetFolder))
                return false;

            string currentFolder = Path.GetDirectoryName(assetPath)!.Replace("\\", "/");
            if (string.Equals(currentFolder, targetFolder, StringComparison.OrdinalIgnoreCase))
                return false;

            string fileName = Path.GetFileName(assetPath);
            string desiredTargetPath = $"{targetFolder}/{fileName}".Replace("\\", "/");
            string targetPath = GetUniquePath(desiredTargetPath);

            string moveError = AssetDatabase.MoveAsset(assetPath, targetPath);
            if (!string.IsNullOrEmpty(moveError))
            {
                warning = $"Move failed:\n{assetPath}\n-> {targetPath}\nError: {moveError}";
                return false;
            }

            return true;
        }

        public static bool TryDelete(
            IEnumerable<string> assetPaths,
            bool askConfirmation = true)
        {
            string[] paths = assetPaths
                .Where(p => !string.IsNullOrEmpty(p))
                .Where(AssetDatabase.AssetPathExists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (paths.Length == 0)
                return false;

            if (askConfirmation)
            {
                string preview = string.Join("\n", paths.Take(10));
                if (paths.Length > 10)
                    preview += "\n...";

                bool confirm = EditorUtility.DisplayDialog(
                    "Delete Assets?",
                    $"Delete {paths.Length} assets?\n\n{preview}",
                    "Delete",
                    "Cancel");

                if (!confirm)
                    return false;
            }

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string path in paths)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            Debug.Log($"Deleted {paths.Length} assets.");
            return true;
        }

        public static bool TryDeleteBySuffix(string suffix, string? targetFolder = null, bool askConfirmation = true)
        {
            if (string.IsNullOrEmpty(suffix))
            {
                Debug.LogWarning("DeleteAssetsWithSuffix called with empty suffix.");
                return false;
            }

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith("Assets/", StringComparison.Ordinal))
                .ToArray();

            string[] matches = allAssetPaths
                .Where(assetPath =>
                {
                    var pathMatch = true;
                    if (targetFolder != null)
                    {
                        string? assetDir = Path.GetDirectoryName(assetPath);
                        string? targetDir = Path.GetDirectoryName(targetFolder);
                        pathMatch =
                            assetDir?.Contains(targetDir, StringComparison.OrdinalIgnoreCase) ??
                            false;
                    }

                    return pathMatch && Path.GetFileNameWithoutExtension(assetPath)
                        .EndsWith(suffix, StringComparison.Ordinal);
                })
                .ToArray();

            if (matches.Length == 0)
            {
                Debug.Log($"No assets found with prefix '{suffix}'.");
                return false;
            }

            return TryDelete(matches, askConfirmation);
        }

        private static bool ShouldSuggestedChangesIgnore(string assetPath)
        {
            string? currentDir = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            // Only auto move files placed in root folder
            return !(currentDir?.EndsWith("Assets", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private static string StripKnownSuffix(string stem, string path, string ext)
        {
            string? suffixForAsset = GetAssetSuffix(path, ext);
            if (string.IsNullOrEmpty(suffixForAsset) ||
                !stem.EndsWith(suffixForAsset, StringComparison.OrdinalIgnoreCase)) return stem;
            return suffixForAsset != null ? stem.Substring(0, stem.Length - suffixForAsset.Length) : stem;
        }

        private static void EnsureFoldersAndFiles(ref int created, ref int existed)
        {
            // Ensure all folders exist.
            foreach (string path in FoldersToAutoCreate)
            {
                if (Ensure(path))
                    created++;
                else
                    existed++;
            }

            EnsureRequiredCodeFiles(ref created);

            if (created > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"Folder structure ensured. Created:{created} Existed:{existed}");
        }

        private static void EnsureRequiredCodeFiles(ref int created)
        {
            // Runtime should have a nullable enable csc.rsp
            string cscRuntimePath = Sanitize(RuntimeCodePath + "csc.rsp");
            if (!File.Exists(cscRuntimePath))
            {
                File.WriteAllText(cscRuntimePath, "-nullable:enable");
                created++;
            }

            // Editor should have a nullable enable csc.rsp
            string cscEditorPath = Sanitize(EditorCodePath + "csc.rsp");
            if (!File.Exists(cscEditorPath))
            {
                File.WriteAllText(cscEditorPath, "-nullable:enable");
                created++;
            }

            // Editor should have asmdef
            string editorAsmdefPath =
                Sanitize(EditorCodePath + $"{Application.productName}.Editor.asmdef");
            if (!File.Exists(editorAsmdefPath))
            {
                var asmdef = $@"
                {{
                    ""name"": ""{Application.productName}.Editor"",
                    ""rootNamespace"": ""{Application.productName}"",
                    ""references"": [
                        ""Konfus.ToolsAndSystems"",
                        ""Konfus.ToolsAndSystems.Editor""
                    ],
                    ""includePlatforms"": [
                        ""Editor""
                    ],
                    ""excludePlatforms"": [],
                    ""allowUnsafeCode"": false,
                    ""overrideReferences"": false,
                    ""precompiledReferences"": [],
                    ""autoReferenced"": true,
                    ""defineConstraints"": [],
                    ""versionDefines"": [],
                    ""noEngineReferences"": false
                }}";
                File.WriteAllText(editorAsmdefPath, asmdef);
                created++;
            }

            // Runtime should have asmdef
            string runtimeAsmdefPath =
                Sanitize(RuntimeCodePath + $"{Application.productName}.asmdef");
            if (!File.Exists(runtimeAsmdefPath))
            {
                var asmdef = $@"
                {{
                    ""name"": ""{Application.productName}"",
                    ""rootNamespace"": ""{Application.productName}"",
                    ""references"": [
                        ""Konfus.ToolsAndSystems""
                    ],
                    ""includePlatforms"": [],
                    ""excludePlatforms"": [],
                    ""allowUnsafeCode"": false,
                    ""overrideReferences"": false,
                    ""precompiledReferences"": [],
                    ""autoReferenced"": true,
                    ""defineConstraints"": [],
                    ""versionDefines"": [],
                    ""noEngineReferences"": false
                }}";
                File.WriteAllText(runtimeAsmdefPath, asmdef);
                created++;
            }
        }
    }
}