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
            EditorGUILayout.HelpBox(
                "FX items are shown as auto-timed timeline tracks. Start/Stop are calculated from item order and effect duration; async tracks are striped and play in parallel.",
                MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawPlayButton();
            DrawPauseButton();
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

            if (fxSystem.IsPlaying) GUI.enabled = false;

            GUI.color = Color.green;
            if (GUILayout.Button(new GUIContent("Play", "Plays effects in runtime play mode or editor preview mode.")))
            {
                if (Application.isPlaying)
                {
                    fxSystem.PlayEffects();
                }
                else
                {
                    FxSystemPreviewController.PlayPreview(fxSystem);
                }
            }

            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }

        private void DrawPauseButton()
        {
            var fxSystem = (FxSystem)target;
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;

            if (!fxSystem.IsPlaying) GUI.enabled = false;

            GUI.color = new Color(1f, 0.82f, 0.1f);
            if (GUILayout.Button(new GUIContent("Pause", "Pauses runtime playback or editor preview without resetting.")))
            {
                if (Application.isPlaying)
                    fxSystem.PauseEffects();
                else
                    FxSystemPreviewController.PausePreview(fxSystem);
            }

            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }

        private void DrawStopButton()
        {
            var fxSystem = (FxSystem)target;
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;

            GUI.color = Color.red;
            if (GUILayout.Button(new GUIContent("Stop", "Fully stops and resets runtime playback or editor preview.")))
            {
                if (Application.isPlaying)
                    fxSystem.StopEffects();
                else
                    FxSystemPreviewController.StopPreview(fxSystem);
            }

            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }
    }
}
