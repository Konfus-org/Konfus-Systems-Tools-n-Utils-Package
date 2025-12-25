using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    [DisallowMultipleComponent]
    public class Player : MonoBehaviour
    {
        [SerializeField]
        private InputActionBinding[] inputBindings = Array.Empty<InputActionBinding>();

        private void OnEnable()
        {
            foreach (InputActionBinding binding in inputBindings)
            {
                if (binding.BoundAction == null) continue;
                binding.BoundAction.actionMap.actionTriggered += OnInput;
                binding.BoundAction.Enable();
            }
        }

        private void OnDisable()
        {
            foreach (InputActionBinding binding in inputBindings)
            {
                if (binding.BoundAction == null) continue;
                binding.BoundAction.actionMap.actionTriggered -= OnInput;
                binding.BoundAction.Disable();
            }
        }

        private void OnInput(InputAction.CallbackContext ctx)
        {
            foreach (InputActionBinding binding in inputBindings)
            {
                binding.Process(ctx);
            }
        }
    }
}