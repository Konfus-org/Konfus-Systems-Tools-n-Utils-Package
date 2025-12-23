using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    [Serializable]
    public struct InputActionBinding
    {
        [SerializeField]
        private InputActionReference? actionReference;

        public bool Matches(InputAction.CallbackContext ctx)
        {
            if (actionReference == null) return false;
            return ctx.action == actionReference.action;
        }
    }
}