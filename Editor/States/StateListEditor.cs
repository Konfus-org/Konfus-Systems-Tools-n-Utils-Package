using System;
using System.Collections.Generic;
using Konfus.States;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.States
{
    [CustomEditor(typeof(StateList))]
    internal class StateListEditor : UnityEditor.Editor
    {
        private SerializedProperty? _availableStates;

        private void OnEnable()
        {
            _availableStates = serializedObject.FindProperty("states");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_availableStates != null)
            {
                EditorGUILayout.PropertyField(_availableStates, new GUIContent("Available States"), true);
            }

            foreach (var message in BuildValidationMessages())
            {
                EditorGUILayout.HelpBox(message, MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private List<string> BuildValidationMessages()
        {
            var messages = new List<string>();
            if (_availableStates == null)
            {
                return messages;
            }

            var seenStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < _availableStates.arraySize; i++)
            {
                var stateProperty = _availableStates.GetArrayElementAtIndex(i);
                var stateName = stateProperty.FindPropertyRelative("stateName")?.stringValue ?? string.Empty;
                var label = $"State {i + 1}";

                if (string.IsNullOrWhiteSpace(stateName))
                {
                    messages.Add($"{label} is empty.");
                    continue;
                }

                if (!seenStates.Add(stateName))
                {
                    messages.Add($"{label} duplicates '{stateName}'.");
                }
            }

            return messages;
        }
    }
}
