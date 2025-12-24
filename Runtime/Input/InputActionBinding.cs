using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    public enum InputConditionType
    {
        Performed,
        Cancelled,
        Both
    }

    [Serializable]
    public sealed class InputActionBinding
    {
        [SerializeField]
        private InputActionReference? action;
        [SerializeField]
        private InputConditionType trigger = InputConditionType.Cancelled;
        [SerializeField]
        private InputTarget target = new();

        public void Process(InputAction.CallbackContext ctx)
        {
            if (IsBoundTo(ctx))
                target.Invoke(ctx);
        }

        private bool IsBoundTo(InputAction.CallbackContext ctx)
        {
            if (!Matches(ctx))
                return false;

            return trigger switch
            {
                InputConditionType.Performed => ctx.performed,
                InputConditionType.Cancelled => ctx.canceled,
                InputConditionType.Both => true,
                _ => false
            };
        }

        private bool Matches(InputAction.CallbackContext ctx)
        {
            if (action == null) return false;
            return ctx.action == action.action;
        }
    }
}