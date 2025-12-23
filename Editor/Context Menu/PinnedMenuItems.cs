using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Editor.Code_Gen;
using Konfus.Editor.Utility;
using Konfus.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Context_Menu
{
    //[InitializeOnLoad]
    internal class PinnedMenuItems
    {
        private const string GeneratedSuffix = "_PinnedMenuItem";

        [MenuItem("Tools/Konfus/Pinned/Edit", priority = 1)]
        private static void OpenWindow()
        {
            ContextMenuPinnedItemsWindow.ShowWindow();
        }

        [MenuItem("Assets/Konfus/Pinned/Edit Pins", priority = 1)]
        private static void OpenContext()
        {
            ContextMenuPinnedItemsWindow.ShowWindow();
        }

        [MenuItem("Tools/Konfus/Pinned/Regenerate Menu Items", priority = 1)]
        private static void GenerateContextMenuItems()
        {
            GenerateContextMenuItems(true);
        }

        public static void GenerateContextMenuItems(bool promptToDelete)
        {
            List<MenuItemReflection.MenuItemInfo> pinnedItems =
                ProjectSettings.LoadPinnedItems(MenuItemReflection.GetAllMenuItems());
            if (pinnedItems.IsNullOrEmpty()) return;

            using var scope = new AssetDatabase.AssetEditingScope();
            ProjectManager.TryDeleteBySuffix(GeneratedSuffix, ProjectManager.EditorGeneratedCodePath, promptToDelete);
            GenerateContextMenuItems(pinnedItems);
        }

        private static void GenerateContextMenuItems(IEnumerable<MenuItemReflection.MenuItemInfo> items)
        {
            CodeGenerator.GenerateScripts((from item in items
                let pinRootName = item.MenuPath.Split('/').First()
                let fullPinName = string.Join("->", item.MenuPath.Split('/').TakeLast(3))
                let pinNameWithoutRoot = string.Join("->", item.MenuPath.Split('/').TakeLast(3))
                    .Replace(pinRootName + "->", string.Empty)
                let scriptName = ProjectManager
                    .Sanitize(string.Join('_', item.MenuPath.Split('/'))).ToPascalCase() + GeneratedSuffix
                let code = $@"#nullable enable
using UnityEditor;
using UnityEngine;

namespace {CodeGenerator.GenerateNamespace("Generated")}
{{
    public static class {scriptName.ToPascalCase()}
    {{
        [MenuItem(""Tools/Konfus/Pinned/All/{fullPinName}"", priority = -1)]
        [MenuItem(""Assets/Konfus/Pinned/All/{fullPinName}"", priority = -1)]
        [MenuItem(""{pinRootName}/Pinned/{pinNameWithoutRoot}"", priority = -1)]
        private static void Open()
        {{
            EditorApplication.ExecuteMenuItem(""{item.MenuPath}"");
        }}
    }}
}}"
                select new CodeGenTemplate(scriptName, code)).ToArray());
        }

        private sealed class ContextMenuPinnedItemsWindow : EditorWindow
        {
            private string[] _allItemPaths = Array.Empty<string>();
            private MenuItemReflection.MenuItemInfo[] _allItems = Array.Empty<MenuItemReflection.MenuItemInfo>();
            private string _explicitPath = "Explicit/Path/Here";
            private List<MenuItemReflection.MenuItemInfo> _pinnedItems = new();
            private Vector2 _pinnedItemsScrollPosition = Vector2.zero;
            private int _selectedItemToPinIndex = -1;

            private void OnEnable()
            {
                _allItems =
                    MenuItemReflection.GetAllMenuItems().OrderBy(i => i.MenuPath).ToArray();
                _allItemPaths = _allItems.Select(i => i.MenuPath).ToArray();
                _pinnedItems = ProjectSettings.LoadPinnedItems(_allItems);
            }

            private void OnGUI()
            {
                if (_pinnedItems.Any())
                {
                    using var scrollView = new EditorGUILayout.ScrollViewScope(_pinnedItemsScrollPosition);
                    _pinnedItemsScrollPosition = scrollView.scrollPosition;
                    EditorGUILayout.LabelField("Pinned Items:", EditorStyles.boldLabel);
                    foreach (MenuItemReflection.MenuItemInfo pinnedItem in _pinnedItems.ToArray())
                    {
                        string menuName = string.Join("->", pinnedItem.MenuPath.Split('/').TakeLast(3));
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(menuName))
                            {
                                if (!MenuItemReflection.TryInvoke(pinnedItem, out Exception? error))
                                    Debug.LogError($"{pinnedItem.MenuPath} failed to execute!\nError: {error}");
                            }

                            if (GUILayout.Button("Remove", GUILayout.Width(80)))
                            {
                                _pinnedItems.Remove(pinnedItem);
                                UpdatePinnedMenuItems();
                            }
                        }
                    }
                }

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                MenuItemReflection.MenuItemInfo? itemToPin =
                    _selectedItemToPinIndex >= 0 && _selectedItemToPinIndex < _allItems.Length
                        ? _allItems[_selectedItemToPinIndex]
                        : null;

                using (new EditorGUILayout.HorizontalScope())
                {
                    _selectedItemToPinIndex = EditorGUILayout.Popup(_selectedItemToPinIndex, _allItemPaths);
                    if (GUILayout.Button("Pin", GUILayout.Width(80)) && itemToPin != null)
                    {
                        _pinnedItems.Add(itemToPin);
                        UpdatePinnedMenuItems();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _explicitPath = EditorGUILayout.TextField(string.Empty, _explicitPath);
                    if (GUILayout.Button("Pin", GUILayout.Width(80)) || HandleEnterKey())
                    {
                        if (string.IsNullOrEmpty(_explicitPath)) return;
                        _pinnedItems.Add(new MenuItemReflection.MenuItemInfo(_explicitPath, false, null));
                        UpdatePinnedMenuItems();
                    }
                }
            }

            public static void ShowWindow()
            {
                var w = GetWindow<ContextMenuPinnedItemsWindow>("Pinned Context Menu Items");
                w.minSize = new Vector2(150, 100);
            }

            private bool HandleEnterKey()
            {
                Event e = Event.current;

                if (e.type != EventType.KeyDown)
                    return false;
                if (e.keyCode != KeyCode.Return && e.keyCode != KeyCode.KeypadEnter)
                    return false;
                if (GUI.GetNameOfFocusedControl() != "EnterField")
                    return false;

                e.Use();
                return true;
            }

            private void UpdatePinnedMenuItems()
            {
                ProjectSettings.SavePinnedItems(_pinnedItems);
                GenerateContextMenuItems();
            }
        }
    }
}