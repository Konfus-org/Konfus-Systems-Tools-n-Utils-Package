using Konfus.Fx_System;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Fx_System
{
    [CustomEditor(typeof(FxSystem))]
    internal class FxEditor : UnityEditor.Editor
    {
        private Texture2D? _fxIcon;

        private void Awake()
        {
            _fxIcon = Resources.Load<Texture2D>("FxSystemIcon");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawEditorInspectorGui();
        }

        private void DrawEditorInspectorGui()
        {
            DrawIcon();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawPlayButton();
            DrawStopButton();
        }

        private void DrawIcon()
        {
            // Set icon
            var fxSystem = (FxSystem)target;
            EditorGUIUtility.SetIconForObject(fxSystem, _fxIcon);
        }

        public override bool RequiresConstantRepaint()
        {
            var fxSystem = (FxSystem)target;
            return fxSystem.IsPlaying;
        }

        private void DrawPlayButton()
        {
            var fxSystem = (FxSystem)target;
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;

            if (!Application.isPlaying || fxSystem.IsPlaying) GUI.enabled = false;

            GUI.color = Color.green;
            if (GUILayout.Button(new GUIContent("Play", "Plays effects, only available in play mode.")))
            {
                fxSystem.StopEffects();
                fxSystem.PlayEffects();
            }

            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }

        private void DrawStopButton()
        {
            var fxSystem = (FxSystem)target;
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;

            if (!Application.isPlaying || fxSystem.IsPlaying) GUI.enabled = false;

            GUI.color = Color.red;
            if (GUILayout.Button(new GUIContent("Stop", "Stops playing effects, only available in play mode.")))
                fxSystem.StopEffects();

            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }
    }
}