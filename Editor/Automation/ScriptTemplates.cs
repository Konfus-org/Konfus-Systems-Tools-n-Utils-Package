using System.IO;
using System.Linq;
using Konfus.Editor.Code_Gen;
using Konfus.Editor.Utility;
using Konfus.Utility.Extensions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Editor.Automation
{
    //[InitializeOnLoad]
    internal static class ScriptTemplates
    {
        private const string GeneratedSuffix = "_ScriptTemplateMenuItem";

        [MenuItem("Tools/Konfus/Script Templates/Regenerate Menu Items", priority = 1)]
        private static void GenerateContextMenuItems()
        {
            GenerateContextMenuItems(true);
        }

        public static void GenerateContextMenuItems(bool promptToDelete)
        {
            CodeGenTemplate[]? templates = CodeGenTemplateLoader.LoadAll();
            if (templates == null) return;

            using var scope = new AssetDatabase.AssetEditingScope();
            ProjectManager.TryDeleteBySuffix(GeneratedSuffix, ProjectManager.EditorGeneratedCodePath, promptToDelete);
            CodeGenerator.GenerateScripts((from template in templates
                let name = ProjectManager.Sanitize(template.Name).ToPascalCase() + GeneratedSuffix
                let code = $@"#nullable enable
using System.IO;
using UnityEditor;
using UnityEngine;
using Konfus.Editor.Code_Gen;
using Konfus.Editor.Utility;

namespace {CodeGenerator.GenerateNamespace("Generated")}
{{
    public static class {name}
    {{
          [MenuItem(""Tools/Konfus/Script Templates/{template.Name}"", priority = 0)]
          [MenuItem(""Assets/Konfus/Script Templates/{template.Name}"", priority = 0)]
          private static void Open()
          {{
              string? folderPath = GetSelectedFolderPath();
              TextInputDialog.Show(""New Script"", ""Name:"", s => 
              {{
                    CodeGenerator.GenerateScript(new CodeGenTemplate(s, @""{template.Content}""), folderPath);
                    AssetDatabase.Refresh();
              }});
          }}

          private static string? GetSelectedFolderPath()
          {{
              Object? obj = Selection.activeObject;
              if (obj == null)
                  return null;
              string? path = AssetDatabase.GetAssetPath(obj);
              if (string.IsNullOrEmpty(path))
                  return null;
              return AssetDatabase.IsValidFolder(path) ? path : Path.GetDirectoryName(path);
          }}
    }}
}}"
                select new CodeGenTemplate(name, code)).ToArray());
        }

        [MenuItem("Tools/Konfus/Script Templates/Preview Templates", priority = 1)]
        private static void OpenWindow()
        {
            ScriptTemplateWindow.ShowWindow();
        }

        [MenuItem("Assets/Konfus/Script Templates/Preview Templates", priority = 1)]
        private static void OpenContext()
        {
            ScriptTemplateWindow.ShowWindow();
        }

        private static string? GetSelectedFolderPath()
        {
            Object? obj = Selection.activeObject;
            if (obj == null)
                return null;
            string? path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
                return null;
            return AssetDatabase.IsValidFolder(path) ? path : Path.GetDirectoryName(path);
        }

        private sealed class ScriptTemplateWindow : EditorWindow
        {
            private Vector2 _scriptTemplateScrollPosition = Vector2.zero;
            private CodeGenTemplate[]? _templates;

            private void OnEnable()
            {
                _templates = CodeGenTemplateLoader.LoadAll()?.Reverse().ToArray();
            }

            private void OnGUI()
            {
                string? folderPath = GetSelectedFolderPath();
                if (_templates == null)
                {
                    EditorGUILayout.HelpBox("No template files found.", MessageType.Info);
                    return;
                }

                using (var scrollView = new EditorGUILayout.ScrollViewScope(_scriptTemplateScrollPosition))
                {
                    _scriptTemplateScrollPosition = scrollView.scrollPosition;
                    var templateToCreate = string.Empty;
                    foreach (CodeGenTemplate t in _templates)
                    {
                        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                        {
                            string scriptName;
                            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                            {
                                EditorGUILayout.LabelField(t.Name);
                                var defaultName = $"New{t.Name}Script";
                                scriptName = EditorGUILayout.TextField(defaultName) ?? defaultName;
                                if (GUILayout.Button("Create", GUILayout.Width(80)))
                                {
                                    CodeGenerator.GenerateScript(
                                        new CodeGenTemplate(scriptName,
                                            string.IsNullOrEmpty(templateToCreate) ? t.Content : templateToCreate),
                                        folderPath);
                                    AssetDatabase.Refresh();
                                }
                            }

                            templateToCreate =
                                EditorGUILayout.TextArea(
                                    CodeGenerator.GenerateCode(new CodeGenTemplate(scriptName, t.Content), folderPath));

                            EditorGUILayout.Space(10);
                        }
                    }
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox(
                    "Select a folder in the Project window to create scripts there. " +
                    "Editor templates will be routed into an Editor folder automatically.",
                    MessageType.Info);
            }

            public static void ShowWindow()
            {
                var w = GetWindow<ScriptTemplateWindow>("Script Templates");
                w.minSize = new Vector2(360, 220);
            }
        }
    }
}