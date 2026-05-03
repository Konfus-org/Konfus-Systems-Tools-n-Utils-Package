using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    [DisallowMultipleComponent]
    public class Possessable : MonoBehaviour
    {
        private const InputConditionType GeneratedInputTriggers =
            InputConditionType.Started | InputConditionType.Performed | InputConditionType.Cancelled;

        public event Action<InputAction.CallbackContext>? InputTriggered;

        [Header("Input Routing")]
        [SerializeField]
        private InputActionAsset? inputActions;
        [SerializeField]
        private PossessableInputBinding[] inputBindings = Array.Empty<PossessableInputBinding>();

        [Header("Possession")]
        [SerializeField]
        private GameObject? possessionCamera;
        [Header("Events")]
        [SerializeField]
        private UnityEvent onPossessed = new();
        [SerializeField]
        private UnityEvent onUnpossessed = new();

        private Possessable? _possessedTarget;
        private Possessable? _possessor;
        private bool _isPlayerControlRoot;

        public Possessable? PossessedTarget => _possessedTarget;
        public Possessable? Possessor => _possessor;
        public Possessable ActivePossessable => GetActiveTarget();

        private void Awake()
        {
            RefreshInputBindings();
            ApplyCameraState();
        }

        public bool Possess(Possessable target)
        {
            if (target == null)
            {
                return false;
            }

            if (target == this)
            {
                return Unpossess();
            }

            if (_possessedTarget == target)
            {
                return true;
            }

            if (_possessedTarget != null && !ReleasePossessedTarget(_possessedTarget))
            {
                return false;
            }

            if (target._possessor != null && target._possessor != this)
            {
                return false;
            }

            target._possessor = this;
            _possessedTarget = target;
            RefreshPlayerControlHierarchy();
            target.onPossessed.Invoke();
            return true;
        }

        public bool PossessFromInteractor(Interactor interactor)
        {
            if (interactor == null)
            {
                return false;
            }

            Possessable? possessor = interactor.GetComponentInParent<Possessable>();
            return possessor != null && possessor.Possess(this);
        }

        public bool Unpossess()
        {
            if (_possessedTarget != null)
            {
                return ReleasePossessedTarget(_possessedTarget);
            }

            if (_possessor != null)
            {
                return _possessor.ReleasePossessedTarget(this);
            }

            return false;
        }

        public void HandleUnpossess(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                Unpossess();
            }
        }

        public void SetInputActions(InputActionAsset? actions)
        {
            inputActions = actions;
            RefreshInputBindings();
        }

        public void SetAsPlayerControlRoot(bool isPlayerControlRoot)
        {
            if (_isPlayerControlRoot == isPlayerControlRoot)
            {
                RefreshPlayerControlHierarchy();
                return;
            }

            _isPlayerControlRoot = isPlayerControlRoot;
            RefreshPlayerControlHierarchy();
        }

        public void HandleInput(InputAction.CallbackContext ctx)
        {
            GetActiveTarget().ReceiveInput(ctx);
        }

        public void Move(Vector2 input)
        {
            GetActiveTarget().DispatchLegacyAction("Move", InputConditionType.Performed, input);
        }

        public void Look(Vector2 input)
        {
            GetActiveTarget().DispatchLegacyAction("Look", InputConditionType.Performed, input);
        }

        public void StartJump()
        {
            GetActiveTarget().DispatchLegacyAction("Jump", InputConditionType.Performed, null);
        }

        public void StopJump()
        {
            GetActiveTarget().DispatchLegacyAction("Jump", InputConditionType.Cancelled, null);
        }

        public void StartSprint()
        {
            GetActiveTarget().DispatchLegacyAction("Sprint", InputConditionType.Performed, null);
        }

        public void StopSprint()
        {
            GetActiveTarget().DispatchLegacyAction("Sprint", InputConditionType.Cancelled, null);
        }

        public void Interact()
        {
            GetActiveTarget().DispatchLegacyAction("Interact", InputConditionType.Cancelled, null);
        }

        private void OnDisable()
        {
            Unpossess();
            ApplyCameraState();
        }

        private void OnValidate()
        {
            RefreshInputBindings();
            ApplyCameraState();
        }

        private Possessable GetActiveTarget()
        {
            Possessable current = this;
            int guard = 0;
            while (current._possessedTarget != null && current._possessedTarget != current && guard < 16)
            {
                current = current._possessedTarget;
                guard++;
            }

            return current;
        }

        private bool ReleasePossessedTarget(Possessable target)
        {
            if (_possessedTarget != target || target == null)
            {
                return false;
            }

            target.DispatchLegacyAction("Move", InputConditionType.Performed, Vector2.zero);
            target.DispatchLegacyAction("Look", InputConditionType.Performed, Vector2.zero);
            target.DispatchLegacyAction("Sprint", InputConditionType.Cancelled, null);
            target.DispatchLegacyAction("Jump", InputConditionType.Cancelled, null);

            _possessedTarget = null;
            if (target._possessor == this)
            {
                target._possessor = null;
            }

            RefreshPlayerControlHierarchy();
            target.RefreshPlayerControlHierarchy();
            target.onUnpossessed.Invoke();
            return true;
        }

        private void ReceiveInput(InputAction.CallbackContext ctx)
        {
            InputTriggered?.Invoke(ctx);

            foreach (PossessableInputBinding binding in inputBindings)
            {
                binding.Process(ctx);
            }
        }

        private void DispatchLegacyAction(string actionName, InputConditionType trigger, object? value)
        {
            foreach (PossessableInputBinding binding in inputBindings)
            {
                if (!binding.Triggers.HasFlag(trigger) || string.IsNullOrWhiteSpace(binding.actionName))
                {
                    continue;
                }

                if (!IsLegacyActionMatch(binding.actionName, actionName))
                {
                    continue;
                }

                InvokeTargets(binding.Targets, trigger, value);
            }
        }

        private void RefreshInputBindings()
        {
            if (inputActions == null)
            {
                return;
            }

            var existingBindings = new System.Collections.Generic.Dictionary<string, PossessableInputBinding>(inputBindings.Length);
            foreach (PossessableInputBinding binding in inputBindings)
            {
                existingBindings[binding.BindingKey] = binding;
            }

            var refreshedBindings = new System.Collections.Generic.List<PossessableInputBinding>();
            foreach (InputActionMap actionMap in inputActions.actionMaps)
            {
                foreach (InputAction action in actionMap.actions)
                {
                    AddOrReuseBinding(refreshedBindings, existingBindings, actionMap.name, action);
                }
            }

            inputBindings = refreshedBindings.ToArray();
        }

        private void AddOrReuseBinding(
            System.Collections.Generic.List<PossessableInputBinding> refreshedBindings,
            System.Collections.Generic.Dictionary<string, PossessableInputBinding> existingBindings,
            string actionMapName,
            InputAction action)
        {
            string actionId = action.id.ToString();
            if (!existingBindings.TryGetValue(actionId, out PossessableInputBinding? binding))
            {
                binding = new PossessableInputBinding();
            }

            binding.Configure($"{actionMapName}/{action.name}", actionId, GeneratedInputTriggers);
            refreshedBindings.Add(binding);
        }

        private static bool IsLegacyActionMatch(string? bindingActionName, string actionName)
        {
            if (string.IsNullOrWhiteSpace(bindingActionName))
            {
                return false;
            }

            string expected = $"Player/{actionName}";
            return string.Equals(bindingActionName, expected, StringComparison.Ordinal) ||
                   bindingActionName.StartsWith($"{expected} ", StringComparison.Ordinal);
        }

        private void RefreshPlayerControlHierarchy()
        {
            ApplyCameraState();
            _possessedTarget?.RefreshPlayerControlHierarchy();
        }

        private void ApplyCameraState()
        {
            if (possessionCamera == null)
            {
                return;
            }

            bool shouldBeActive = false;
            Possessable? playerControlRoot = GetPlayerControlRoot();
            if (playerControlRoot != null)
            {
                shouldBeActive = playerControlRoot.GetActiveTarget() == this;
            }

            if (possessionCamera.activeSelf != shouldBeActive)
            {
                possessionCamera.SetActive(shouldBeActive);
            }
        }

        private Possessable? GetPlayerControlRoot()
        {
            if (_isPlayerControlRoot)
            {
                return this;
            }

            return _possessor?.GetPlayerControlRoot();
        }

        private static void InvokeTargets(InputTarget[] targets)
        {
            foreach (InputTarget target in targets)
            {
                target.Invoke();
            }
        }

        private static void InvokeTargets(InputTarget[] targets, object? value)
        {
            foreach (InputTarget target in targets)
            {
                target.Invoke(value);
            }
        }

        private static void InvokeTargets(InputTarget[] targets, InputConditionType trigger, object? value)
        {
            foreach (InputTarget target in targets)
            {
                target.Invoke(trigger, value);
            }
        }
    }
}
