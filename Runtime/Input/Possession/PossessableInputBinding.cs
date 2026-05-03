using System;
using Konfus.Utility.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    [Serializable]
    public sealed class PossessableInputBinding
    {
        [ReadOnly]
        public string? actionName;

        [ReadOnly]
        [SerializeField]
        private string? actionId;
        [SerializeField]
        private InputConditionType triggers =
            InputConditionType.Started | InputConditionType.Performed | InputConditionType.Cancelled;
        [SerializeField]
        private InputTarget[] targets = Array.Empty<InputTarget>();

        public string BindingKey => actionId ?? string.Empty;
        public string? ActionId => actionId;
        public InputConditionType Triggers => triggers;
        public InputTarget[] Targets => targets;

        public void Configure(string displayName, string id, InputConditionType trigger)
        {
            actionName = displayName;
            actionId = id;
            triggers = trigger;
        }

        public void Process(InputAction.CallbackContext ctx)
        {
            if (!Matches(ctx))
            {
                return;
            }

            foreach (InputTarget target in targets)
            {
                target.Invoke(ctx);
            }
        }

        public bool Matches(InputAction.CallbackContext ctx)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                return false;
            }

            if (!string.Equals(ctx.action.id.ToString(), actionId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (triggers.HasFlag(InputConditionType.Started) && ctx.started) return true;
            if (triggers.HasFlag(InputConditionType.Performed) && ctx.performed) return true;
            return triggers.HasFlag(InputConditionType.Cancelled) && ctx.canceled;
        }
    }
}
