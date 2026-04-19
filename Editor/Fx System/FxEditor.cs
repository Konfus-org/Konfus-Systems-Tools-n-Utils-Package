using Konfus.Fx_System;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Fx_System
{
    [CustomEditor(typeof(FxSystem))]
    internal class FxEditor : UnityEditor.Editor
    {
        private Texture2D? _fxIcon;
        private bool _showEvents = true;

        private void Awake()
        {
            _fxIcon = Resources.Load<Texture2D>("FxSystemIcon");
        }

        public override void OnInspectorGUI()
        {
            DrawIcon();
            serializedObject.Update();

            SerializedProperty? scriptProperty = serializedObject.FindProperty("m_Script");
            if (scriptProperty != null)
            {
                bool previousEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(scriptProperty);
                GUI.enabled = previousEnabled;
            }

            EditorGUILayout.Space(2f);
            DrawWarningMessage();
            EditorGUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject,
                "m_Script",
                "fxItems",
                "startedPlaying",
                "stoppedPlaying",
                "finishedPlaying");
            DrawTracksProperty(serializedObject);
            EditorGUILayout.Space(4f);
            DrawEventsFoldout(serializedObject);
            EditorGUILayout.Space(6f);
            DrawTransportButtons();
            EditorGUILayout.Space(4f);
            DrawResetButton((FxSystem)target);

            serializedObject.ApplyModifiedProperties();
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
            return fxSystem.IsPlaying || fxSystem.IsPaused;
        }

        private void DrawWarningMessage()
        {
            EditorGUILayout.HelpBox(
                "Effects are shown as auto-timed timeline tracks. Start/Stop are calculated from item order and effect duration; async tracks are striped and play in parallel.",
                MessageType.Warning);
        }

        private static void DrawTracksProperty(SerializedObject so)
        {
            SerializedProperty? tracksProperty = so.FindProperty("fxItems");
            if (tracksProperty == null) return;
            EditorGUILayout.PropertyField(tracksProperty, new GUIContent("Effects"), true);
        }

        private void DrawEventsFoldout(SerializedObject so)
        {
            SerializedProperty? startedPlaying = so.FindProperty("startedPlaying");
            SerializedProperty? stoppedPlaying = so.FindProperty("stoppedPlaying");
            SerializedProperty? finishedPlaying = so.FindProperty("finishedPlaying");

            if (startedPlaying == null && stoppedPlaying == null && finishedPlaying == null) return;

            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                if (startedPlaying != null) EditorGUILayout.PropertyField(startedPlaying);
                if (stoppedPlaying != null) EditorGUILayout.PropertyField(stoppedPlaying);
                if (finishedPlaying != null) EditorGUILayout.PropertyField(finishedPlaying);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawTransportButtons()
        {
            var fxSystem = (FxSystem)target;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawPlayButton(fxSystem);
            GUILayout.Space(4f);
            DrawPauseButton(fxSystem);
            GUILayout.Space(4f);
            DrawStopButton(fxSystem);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPlayButton(FxSystem fxSystem)
        {
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;

            if (fxSystem.IsPlaying) GUI.enabled = false;

            GUI.color = Color.green;
            if (GUILayout.Button(
                    GetIconContent("Play", "Plays effects. Resumes when paused; otherwise starts from reset state.",
                        "d_PlayButton", "PlayButton"),
                    EditorStyles.miniButton,
                    GUILayout.Width(36f),
                    GUILayout.Height(24f)))
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

        private void DrawPauseButton(FxSystem fxSystem)
        {
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;

            if (!fxSystem.IsPlaying) GUI.enabled = false;

            GUI.color = new Color(1f, 0.82f, 0.1f);
            if (GUILayout.Button(
                    GetIconContent("Pause", "Pauses without resetting.",
                        "d_PauseButton", "PauseButton"),
                    EditorStyles.miniButton,
                    GUILayout.Width(36f),
                    GUILayout.Height(24f)))
            {
                if (Application.isPlaying)
                    fxSystem.PauseEffects();
                else
                    FxSystemPreviewController.PausePreview(fxSystem);
            }

            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }

        private void DrawStopButton(FxSystem fxSystem)
        {
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;
            bool canStop = fxSystem.IsPlaying || fxSystem.IsPaused;

            if (!canStop) GUI.enabled = false;

            GUI.color = Color.red;
            if (GUILayout.Button(
                    GetIconContent("Stop", "Stops playback without resetting current effect results.",
                        "d_PreMatQuad", "PreMatQuad", "d_winbtn_mac_close", "winbtn_mac_close"),
                    EditorStyles.miniButton,
                    GUILayout.Width(36f),
                    GUILayout.Height(24f)))
            {
                if (Application.isPlaying)
                    fxSystem.StopEffects();
                else
                    FxSystemPreviewController.StopPreview(fxSystem);
            }

            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }

        private void DrawResetButton(FxSystem fxSystem)
        {
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;

            GUI.color = new Color(1f, 0.82f, 0.1f);
            if (GUILayout.Button(new GUIContent("Reset", "Fully stops and resets to baseline state.")))
            {
                if (Application.isPlaying)
                    fxSystem.ResetEffects();
                else
                    FxSystemPreviewController.ResetPreview(fxSystem);
            }

            GUI.color = previousColor;
            GUI.enabled = previousEnabled;
        }

        private static GUIContent GetIconContent(string fallbackText, string tooltip, params string[] iconNames)
        {
            for (int i = 0; i < iconNames.Length; i++)
            {
                GUIContent iconContent = EditorGUIUtility.IconContent(iconNames[i]);
                if (iconContent.image == null) continue;
                iconContent.tooltip = tooltip;
                return iconContent;
            }

            return new GUIContent(fallbackText, tooltip);
        }
    }
}
