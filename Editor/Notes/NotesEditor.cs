using Konfus.Notes;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Notes
{
    [CustomEditor(typeof(Note), true)]
    internal class NotesEditor : UnityEditor.Editor
    {
        private Texture2D? _noteIcon;

        private void Awake()
        {
            _noteIcon = Resources.Load<Texture2D>("NoteIcon");
        }

        public override void OnInspectorGUI()
        {
            DrawInspectorGui();
        }

        private void DrawInspectorGui()
        {
            var note = (Note)target;

            // Set icon
            EditorGUIUtility.SetIconForObject(note, _noteIcon);

            // Draw note text area
            var textAreaStyle = new GUIStyle(GUI.skin.textArea);
            textAreaStyle.normal.background = MakeTex(2, 2, Color.yellow);
            textAreaStyle.focused.background = MakeTex(2, 2, Color.yellow);
            note.Text = EditorGUILayout.TextArea(note.Text, GUILayout.Height(200));
        }

        // Helper method to create a texture of a given color
        private Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}