using System;
using UnityEngine;

namespace Konfus.States
{
    [Serializable]
    public class StateReference : ISerializationCallbackReceiver
    {
        [SerializeField] private string stateName = string.Empty;

        public StateReference()
        {
        }

        public StateReference(string stateName)
        {
            this.stateName = stateName ?? string.Empty;
        }

        public string StateName => stateName;

        public bool HasValue => !string.IsNullOrWhiteSpace(stateName);

        public void Set(string value)
        {
            stateName = value ?? string.Empty;
        }

        public override string ToString()
        {
            return stateName;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            stateName ??= string.Empty;
        }
    }
}
