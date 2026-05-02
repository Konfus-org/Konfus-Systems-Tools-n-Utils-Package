using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Konfus.Utility.Attributes;

namespace Konfus.States
{
    public class StateController : MonoBehaviour, ISerializationCallbackReceiver
    {
        [Serializable]
        public class StateData
        {
            [SerializeField] private StateReference state = new();
            [SerializeField, HideInInspector] private string stateName = string.Empty;
            [SerializeField] private UnityEvent onEnterState = new();
            [SerializeField] private UnityEvent onExitState = new();

            public StateReference State => state;
            public UnityEvent OnEnterState => onEnterState;
            public UnityEvent OnExitState => onExitState;

            public void MigrateLegacyData()
            {
                if (!state.HasValue && !string.IsNullOrWhiteSpace(stateName))
                {
                    state.Set(stateName);
                }
            }
        }

        [SerializeField] private StateList? masterStateList;
        [SerializeField] private StateReference initialStateReference = new();
        [SerializeField, HideInInspector] private string initialState = string.Empty;
        [SerializeField, ReadOnly] private string currentState = string.Empty;
        [SerializeField] private List<StateData> stateEvents = new();

        private readonly Dictionary<string, StateData> _stateMap = new(StringComparer.OrdinalIgnoreCase);

        public StateList? MasterStateList => masterStateList;
        public string GetCurrentState() => currentState;
        public IReadOnlyList<string> GetAvailableStateNames() => masterStateList?.AvailableStateNames ?? Array.Empty<string>();

        private void Awake()
        {
            MigrateLegacyData();
            RebuildStateMap();

            if (initialStateReference.HasValue)
            {
                ChangeState(initialStateReference);
            }
        }

        private void OnValidate()
        {
            MigrateLegacyData();
        }

        public void EvaluateAndChangeState(StateExpression expression)
        {
            if (expression == null)
            {
                Debug.LogWarning($"No state expression was provided for '{name}'.", this);
                return;
            }

            var validationErrors = expression.Validate(GetAvailableStateNames());
            if (validationErrors.Count > 0)
            {
                Debug.LogWarning(
                    $"State expression on '{name}' is invalid:\n{string.Join("\n", validationErrors)}",
                    this);
                return;
            }

            if (expression.TryEvaluate(currentState, out var nextState))
            {
                ChangeState(nextState);
            }
        }

        public void EvaluateAndChangeState(string conditionString)
        {
            if (!StateExpression.TryParse(conditionString, out var expression, out var error))
            {
                Debug.LogWarning($"Failed to parse state expression on '{name}': {error}", this);
                return;
            }

            EvaluateAndChangeState(expression);
        }

        public void ChangeState(StateReference stateReference)
        {
            if (stateReference == null || !stateReference.HasValue)
            {
                Debug.LogWarning($"Cannot change to an empty state on '{name}'.", this);
                return;
            }

            ChangeState(stateReference.StateName);
        }

        public void ChangeState(string newState)
        {
            if (string.IsNullOrWhiteSpace(newState))
            {
                Debug.LogWarning($"Cannot change to an empty state on '{name}'.", this);
                return;
            }

            if (string.Equals(currentState, newState, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!IsKnownState(newState))
            {
                Debug.LogWarning($"State '{newState}' is not defined in the assigned state list.", this);
            }

            if (!string.IsNullOrEmpty(currentState) && _stateMap.TryGetValue(currentState, out var currentData))
            {
                currentData.OnExitState?.Invoke();
            }

            currentState = newState;

            if (_stateMap.TryGetValue(currentState, out var newData))
            {
                newData.OnEnterState?.Invoke();
            }
            else
            {
                Debug.LogWarning($"State '{currentState}' has no matching state event entry on '{name}'.", this);
            }
        }

        private void RebuildStateMap()
        {
            _stateMap.Clear();

            foreach (var data in stateEvents.Where(static data => data != null))
            {
                data.MigrateLegacyData();
                if (!data.State.HasValue)
                {
                    continue;
                }

                var stateName = data.State.StateName;
                if (!_stateMap.ContainsKey(stateName))
                {
                    _stateMap.Add(stateName, data);
                }
            }
        }

        private bool IsKnownState(string stateName)
        {
            return masterStateList == null || masterStateList.ContainsState(stateName);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            MigrateLegacyData();
        }

        private void MigrateLegacyData()
        {
            if (!initialStateReference.HasValue && !string.IsNullOrWhiteSpace(initialState))
            {
                initialStateReference.Set(initialState);
            }

            foreach (var stateData in stateEvents.Where(static stateData => stateData != null))
            {
                stateData.MigrateLegacyData();
            }
        }
    }
}
