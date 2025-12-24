using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class Player : MonoBehaviour
    {
        [SerializeField]
        private InputActionBinding[] inputBindings = Array.Empty<InputActionBinding>();

        private PlayerInput? _playerInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            if (!_playerInput) return;
            _playerInput.onActionTriggered += OnInput;
        }

        private void OnDisable()
        {
            if (!_playerInput) return;
            _playerInput.onActionTriggered -= OnInput;
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