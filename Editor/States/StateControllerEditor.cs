using System;
using System.Collections.Generic;
using Konfus.States;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.States
{
    [CustomEditor(typeof(StateController))]
    internal class StateControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty? _masterStateList;
        private SerializedProperty? _initialState;
        private SerializedProperty? _currentState;
        private SerializedProperty? _stateEvents;

        private void OnEnable()
        {
            _masterStateList = serializedObject.FindProperty("masterStateList");
            _initialState = serializedObject.FindProperty("initialStateReference");
            _currentState = serializedObject.FindProperty("currentState");
            _stateEvents = serializedObject.FindProperty("stateEvents");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_masterStateList != null)
            {
                EditorGUILayout.PropertyField(_masterStateList);
            }

            var stateList = _masterStateList?.objectReferenceValue as StateList;
            if (stateList == null)
            {
                EditorGUILayout.HelpBox("Assign a State List before configuring states on this controller.", MessageType.Warning);
            }

            if (_initialState != null)
            {
                EditorGUILayout.PropertyField(_initialState, new GUIContent("Initial State"));
            }

            if (_currentState != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(_currentState);
                }
            }

            if (_stateEvents != null)
            {
                EditorGUILayout.PropertyField(_stateEvents, true);
            }

            foreach (var message in BuildValidationMessages(stateList))
            {
                EditorGUILayout.HelpBox(message, MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private List<string> BuildValidationMessages(StateList? stateList)
        {
            var messages = new List<string>();

            var initialStateName = StateEditorUtility.GetStateReferenceName(_initialState);
            if (stateList != null && !string.IsNullOrWhiteSpace(initialStateName) && !stateList.ContainsState(initialStateName))
            {
                messages.Add($"Initial state '{initialStateName}' is not defined in the assigned state list.");
            }

            if (_stateEvents == null)
            {
                return messages;
            }

            var seenStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < _stateEvents.arraySize; i++)
            {
                var stateEventProperty = _stateEvents.GetArrayElementAtIndex(i);
                var stateProperty = stateEventProperty.FindPropertyRelative("state");
                var stateName = StateEditorUtility.GetStateReferenceName(stateProperty);
                var label = $"State Event {i + 1}";

                if (string.IsNullOrWhiteSpace(stateName))
                {
                    messages.Add($"{label} is missing its state.");
                    continue;
                }

                if (stateList != null && !stateList.ContainsState(stateName))
                {
                    messages.Add($"{label} references '{stateName}', which is not defined in the assigned state list.");
                }

                if (!seenStates.Add(stateName))
                {
                    messages.Add($"{label} duplicates the state '{stateName}'.");
                }
            }

            return messages;
        }
    }
}
