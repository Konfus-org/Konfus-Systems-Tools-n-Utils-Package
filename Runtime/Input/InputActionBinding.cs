using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    [Flags]
    public enum InputConditionType
    {
        None = 0,
        Started = 1 << 0,
        Performed = 1 << 1,
        Cancelled = 1 << 2
    }

    [Serializable]
    public sealed class InputActionBinding
    {
        [SerializeField]
        private InputActionReference? action;
        [SerializeField]
        private InputConditionType triggers = InputConditionType.Cancelled;
        [SerializeField]
        private InputTarget[] targets = Array.Empty<InputTarget>();

        public InputAction? BoundAction => action?.action;

        public void Process(InputAction.CallbackContext ctx)
        {
            if (!ShouldProcess(ctx)) return;

            foreach (InputTarget target in targets)
            {
                target.Invoke(ctx);
            }
        }

        private bool ShouldProcess(InputAction.CallbackContext ctx)
        {
            if (!Matches(ctx))
                return false;
            if (triggers.HasFlag(InputConditionType.Started) && ctx.started) return true;
            if (triggers.HasFlag(InputConditionType.Performed) && ctx.performed) return true;
            return triggers.HasFlag(InputConditionType.Cancelled) && ctx.canceled;
        }

        private bool Matches(InputAction.CallbackContext ctx)
        {
            if (action == null) return false;
            return ctx.action == action.action;
        }
    }
}