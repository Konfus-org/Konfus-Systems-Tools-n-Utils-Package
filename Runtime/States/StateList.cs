using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Konfus.States
{
    [CreateAssetMenu(fileName = "NewStateList", menuName = "State Machine/State List")]
    public class StateList : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<StateDefinition> states = new();
        [SerializeField, HideInInspector] private List<string> availableStates = new();

        public IReadOnlyList<StateDefinition> AvailableStates => states;

        public IReadOnlyList<string> AvailableStateNames =>
            states
                .Where(static state => state != null && state.HasValue)
                .Select(static state => state.StateName)
                .ToList();

        public bool ContainsState(string stateName)
        {
            return states.Any(state =>
                state != null &&
                state.HasValue &&
                string.Equals(state.StateName, stateName, System.StringComparison.OrdinalIgnoreCase));
        }

        private void OnValidate()
        {
            MigrateLegacyStates();
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            MigrateLegacyStates();
        }

        private void MigrateLegacyStates()
        {
            if ((states == null || states.Count == 0) && availableStates is { Count: > 0 })
            {
                states = availableStates.Select(stateName => new StateDefinition(stateName)).ToList();
            }
        }
    }
}
