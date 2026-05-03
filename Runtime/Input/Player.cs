using System;
using Konfus.Utility.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    [Provide]
    [DisallowMultipleComponent]
    public class Player : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset? inputActions;

        private DefaultPlayerPossessableProvider? _defaultPossessableProvider;
        private bool _hasWarnedMissingDefaultPossessable;

        public static Player? Instance { get; private set; }
        public event Action<InputAction.CallbackContext>? InputTriggered;

        public InputActionAsset? InputActions => inputActions;
        public Possessable? DefaultPossessable => _defaultPossessableProvider?.Possessable;
        public Possessable? ActivePossessable => DefaultPossessable?.ActivePossessable;

        [Inject]
        private void Inject(DefaultPlayerPossessableProvider defaultPossessableProvider)
        {
            _defaultPossessableProvider = defaultPossessableProvider;
            ConfigureDefaultPossessable();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Multiple {nameof(Player)} instances found. Replacing the previous global instance.", this);
            }

            Instance = this;
        }

        private void OnEnable()
        {
            if (inputActions == null)
            {
                return;
            }

            foreach (InputActionMap actionMap in inputActions.actionMaps)
            {
                actionMap.actionTriggered += OnInput;
                actionMap.Enable();
            }

            ConfigureDefaultPossessable();
        }

        private void Start()
        {
            ConfigureDefaultPossessable();
        }

        private void OnValidate()
        {
            if (DefaultPossessable != null)
            {
                DefaultPossessable.SetInputActions(inputActions);
            }
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                foreach (InputActionMap actionMap in inputActions.actionMaps)
                {
                    actionMap.actionTriggered -= OnInput;
                    actionMap.Disable();
                }
            }

            DefaultPossessable?.SetAsPlayerControlRoot(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnInput(InputAction.CallbackContext ctx)
        {
            InputTriggered?.Invoke(ctx);
            DefaultPossessable?.HandleInput(ctx);
        }

        private void ConfigureDefaultPossessable()
        {
            if (_defaultPossessableProvider == null || _defaultPossessableProvider.Possessable == null)
            {
                if (!_hasWarnedMissingDefaultPossessable && Application.isPlaying)
                {
                    Debug.LogWarning($"{nameof(Player)} could not find a default possessable through dependency injection.", this);
                    _hasWarnedMissingDefaultPossessable = true;
                }

                return;
            }

            _hasWarnedMissingDefaultPossessable = false;
            _defaultPossessableProvider.Possessable.SetInputActions(inputActions);
            _defaultPossessableProvider.Possessable.SetAsPlayerControlRoot(isActiveAndEnabled);
        }
    }
}
