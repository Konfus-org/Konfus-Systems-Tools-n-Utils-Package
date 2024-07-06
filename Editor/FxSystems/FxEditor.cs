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
        }
        
        private void DrawIcon()
        {
            // Set icon
            var fxSystem = (FxSystem)target;
            EditorGUIUtility.SetIconForObject(fxSystem, _fxIcon);
        }
    }
}
