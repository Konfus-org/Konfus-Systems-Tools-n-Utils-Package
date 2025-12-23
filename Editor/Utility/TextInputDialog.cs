using System;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Utility
{
    public class TextInputDialog : EditorWindow
    {
        private string? _input;
        private string _label = "Enter text:";
        private Action<string>? _onConfirm;

        private void OnGUI()
        {
            GUILayout.Label(_label, EditorStyles.boldLabel);
            _input = EditorGUILayout.TextField(_input);

            GUILayout.Space(10);

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cancel")) Close();
                if (!GUILayout.Button("OK")) return;
                if (string.IsNullOrEmpty(_input))
                {
                    EditorGUILayout.HelpBox("Empty input is not allowed, please try again.", MessageType.Info);
                    return;
                }

                _onConfirm?.Invoke(_input);
                Close();
            }
        }

        public static void Show(string title, string label, Action<string> onConfirm)
        {
            var window = CreateInstance<TextInputDialog>();
            window.titleContent = new GUIContent(title);
            window._label = label;
            window._onConfirm = onConfirm;
            window.ShowUtility();
            window.minSize = new Vector2(100, 80);
            window.maxSize = new Vector2(250, 180);
        }
    }
}