using System;
using UnityEngine;

namespace Konfus.States
{
    [Serializable]
    public class StateDefinition
    {
        [SerializeField] private string stateName = string.Empty;

        public StateDefinition()
        {
        }

        public StateDefinition(string stateName)
        {
            this.stateName = stateName ?? string.Empty;
        }

        public string StateName => stateName;

        public bool HasValue => !string.IsNullOrWhiteSpace(stateName);

        public void Set(string value)
        {
            stateName = value ?? string.Empty;
        }
    }
}
