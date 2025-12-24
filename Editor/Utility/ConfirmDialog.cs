using System;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Utility
{
    public sealed class ConfirmDialog : EditorWindow
    {
        private bool _doNotAskAgain;
        private string _message = "Confirm";
        private Action<bool>? _onResult;
        private string _prefsKey = "";
        private Vector2 _scrollPosition;

        private void OnGUI()
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollView.scrollPosition;
                EditorGUILayout.LabelField(_message, EditorStyles.wordWrappedLabel);
            }

            GUILayout.Space(10);

            using (new GUILayout.HorizontalScope())
            {
                _doNotAskAgain = EditorGUILayout.ToggleLeft(
                    "Don't ask again",
                    _doNotAskAgain
                );

                if (GUILayout.Button("No", GUILayout.Width(80)))
                    CloseWithResult(false);
                if (GUILayout.Button("Yes", GUILayout.Width(80)))
                    CloseWithResult(true);
            }
        }

        public static void Show(
            string title,
            string message,
            Action<bool> onResult)
        {
            // Auto-accept if user opted out previously
            string editorPrefsKey = ProjectSettings.DoNotAskAgainId + title;
            if (EditorPrefs.GetBool(editorPrefsKey, false))
            {
                onResult?.Invoke(EditorPrefs.GetBool(editorPrefsKey + ".Choice", false));
                return;
            }

            var window = CreateInstance<ConfirmDialog>();
            window.titleContent = new GUIContent(title);
            window._message = message;
            window._prefsKey = editorPrefsKey;
            window._onResult = onResult;

            window.ShowUtility();
        }

        private void CloseWithResult(bool accepted)
        {
            if (_doNotAskAgain)
            {
                EditorPrefs.SetBool(_prefsKey + ".Choice", accepted);
                EditorPrefs.SetBool(_prefsKey, true);
            }

            // Important: invoke the callback and close on the next tick.
            // This avoids edge cases where Unity is mid-IMGUI event and the window refuses to close.
            EditorApplication.delayCall += () =>
            {
                try
                {
                    _onResult?.Invoke(accepted);
                }
                finally
                {
                    Close();
                }
            };
        }
    }
}