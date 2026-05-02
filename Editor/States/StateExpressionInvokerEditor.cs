using Konfus.States;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.States
{
    [CustomEditor(typeof(StateExpressionInvoker))]
    internal class StateExpressionInvokerEditor : UnityEditor.Editor
    {
        private SerializedProperty? _stateController;
        private SerializedProperty? _expression;

        private void OnEnable()
        {
            _stateController = serializedObject.FindProperty("stateController");
            _expression = serializedObject.FindProperty("expression");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_stateController != null)
            {
                EditorGUILayout.PropertyField(_stateController);
            }

            if (_expression != null)
            {
                EditorGUILayout.PropertyField(_expression, GUIContent.none, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
