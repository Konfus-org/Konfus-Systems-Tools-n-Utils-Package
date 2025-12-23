using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    public enum InputConditionType
    {
        Performed,
        Cancelled
    }

    [Serializable]
    public sealed class InputCondition
    {
        [SerializeField]
        private InputActionBinding inputAction;
        [SerializeField]
        private InputConditionType type = InputConditionType.Cancelled;

        public bool Evaluate(InputAction.CallbackContext ctx)
        {
            if (!inputAction.Matches(ctx))
                return false;

            return type switch
            {
                InputConditionType.Performed => ctx.performed,
                InputConditionType.Cancelled => ctx.canceled,
                _ => false
            };
        }
    }
}