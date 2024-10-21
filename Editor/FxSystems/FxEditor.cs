using System;
using Konfus.Systems.FX;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.FxSystems
{
    [CustomEditor(typeof(FxSystem))]
    public class FxEditor : UnityEditor.Editor
    {
        private Texture2D _fxIcon;
        private Type[] _availableEffectTypes;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawEditorInspectorGui();
        }

        private void Awake()
        {
            _fxIcon = Resources.Load<Texture2D>("FxSystemIcon");
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

        private void DrawPlayButton()
        {
            if (!Application.isPlaying)
            {
                GUI.enabled = false;
            }
            
            if (GUILayout.Button(new GUIContent("Play", "Plays effects, only available in play mode.")))
            {
                var fxSystem = (FxSystem)target;
                fxSystem.StopEffects();
                fxSystem.PlayEffects();
            }
        }

        private void DrawStopButton()
        {
            if (!Application.isPlaying)
            {
                GUI.enabled = false;
            }
            
            if (GUILayout.Button(new GUIContent("Stop", "Stops playing effects, only available in play mode.")))
            {
                var fxSystem = (FxSystem)target;
                fxSystem.StopEffects();
            }
        }
    }
}
