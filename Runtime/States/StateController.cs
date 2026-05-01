using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Konfus.Utility.Attributes;

namespace Konfus.States
{
    public class StateController : MonoBehaviour
    {
        [Serializable]
        public struct StateData
        {
            public string stateName;
            public UnityEvent onEnterState;
            public UnityEvent onExitState;
        }

        // Link to the ScriptableObject that stores all valid strings
        [SerializeField] private StateList? masterStateList;
        
        [SerializeField] private string? initialState;
        
        [SerializeField, ReadOnly] private string? currentState;

        [SerializeField] private List<StateData> stateEvents = new();

        private Dictionary<string, StateData> _stateMap = new();

        private void Awake()
        {
            // Build the dictionary
            foreach (var data in stateEvents)
            {
                if (!string.IsNullOrEmpty(data.stateName) && !_stateMap.ContainsKey(data.stateName))
                {
                    _stateMap.Add(data.stateName, data);
                }
            }

            // Initialize state
            if (!string.IsNullOrEmpty(initialState))
            {
                ChangeState(initialState);
            }
        }
        
        public string GetCurrentState() => currentState;
            
        /// <summary>
        /// Parses the conditional string and changes the state accordingly.
        /// Supports formats like: "if Open: Closed else: Open" or "if Open: Closed elif Closed: Open"
        /// </summary>
        public void EvaluateAndChangeState(string conditionString)
        {
            // Normalize string and remove redundant "if" / "elif" labels
            string cleanString = conditionString.Replace("if ", "").Replace("elif ", "");
            
            // Split conditions and else clauses
            string[] parts = cleanString.Split(new[] { " else:", " elif:" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                // Split the "Condition : NextState" rule
                string[] rule = part.Split(':');
                if (rule.Length != 2) continue;

                string conditionState = rule[0].Trim();
                string targetState = rule[1].Trim();

                // If the condition matches the current state, apply the target state and exit
                if (conditionState.Equals(currentState, StringComparison.OrdinalIgnoreCase))
                {
                    ChangeState(targetState);
                    return;
                }
            }

            // Handle the default fallback (e.g., if an "else:" is present at the end)
            if (conditionString.Contains("else:"))
            {
                int elseIndex = conditionString.LastIndexOf("else:", StringComparison.Ordinal);
                string elseTarget = conditionString.Substring(elseIndex + 5).Trim();
                ChangeState(elseTarget);
            }
        }

        public void ChangeState(string newState)
        {
            if (currentState == newState) return; // Optional: prevents re-triggering if already in state

            // 1. Exit previous state
            if (!string.IsNullOrEmpty(currentState) && _stateMap.TryGetValue(currentState, out var currentData))
            {
                currentData.onExitState?.Invoke();
            }

            currentState = newState;

            // 2. Enter new state
            if (_stateMap.TryGetValue(currentState, out var newData))
            {
                newData.onEnterState?.Invoke();
            }
            else
            {
                Debug.LogWarning($"State '{currentState}' not found in state controller events.");
            }
        }
    }   
}