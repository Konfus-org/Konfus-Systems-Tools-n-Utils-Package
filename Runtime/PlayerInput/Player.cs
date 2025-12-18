using System;
using Konfus.Characters;
using Konfus.Pickups;
using Konfus.Utility.Design_Patterns;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Konfus.PlayerInput
{
    public class Player : PersistentSingleton<Player>
    {
        private const string CONTROLLER_LAYOUT_NAME = "Stick";

        [Header("Events")]
        public InteractEvent onInteract;
        public MoveEvent onMove;
        public JumpEvent onJump;
        public AimEvent onAim;
        public FireEvent onPrimaryFire;
        public FireEvent onSecondaryFire;

        [Header("References")]
        [SerializeField]
        private Camera cam;
        [SerializeField]
        private Reticle reticle;
        [SerializeField]
        private CinemachineTargetGroup targetGroup;

        private PlayerPossessable _defaultPossessed;
        private PlayerPossessable _currentPossessed;
        private Vector2 _lastAimInput;
        private bool _noAimInput;
        private bool _usingController;
        
        public Reticle Reticle => reticle;

        public void SetDefaultPossessed(PlayerPossessable possessed)
        {
            // Possess whatever our default character or tank or whatever is...
            _defaultPossessed = possessed;
            PossessDefault();
        }
        
        public void PossessDefault()
        {
            Possess(_defaultPossessed);
        }

        public void Possess(PlayerPossessable possessable)
        {
            // Set new possessed
            if (_currentPossessed)
            {
                // If we are possessing a vehicle parent our default to the vehicle
                if (possessable.gameObject.CompareTag("Vehicle"))
                {
                    // TODO: WILL BREAK IN MULTIPLAYER!!!! Parenting the possessed (player object as far as networking is concerned) seems to throw errors...
                    _defaultPossessed.transform.parent = possessable.transform;
                }
                
                _currentPossessed.onPlayerUnpossess.Invoke();
            }
            _currentPossessed = possessable;
            
            // Clear old listeners
            onAim.RemoveAllListeners();
            onMove.RemoveAllListeners();
            onJump.RemoveAllListeners();
            onInteract.RemoveAllListeners();
            onPrimaryFire.RemoveAllListeners();
            onSecondaryFire.RemoveAllListeners();
                
            // Always add reticle back... possessee can decide whether or not to turn it on
            onAim.AddListener(reticle.MoveTo);
            
            // Add new listeners
            onAim.AddListener(possessable.Aim);
            onMove.AddListener(possessable.Move);
            onJump.AddListener(possessable.Jump);
            onInteract.AddListener(possessable.Interact);
            onPrimaryFire.AddListener(possessable.PrimaryAction);
            onSecondaryFire.AddListener(possessable.SecondaryAction);
            
            // Update target group
            if (targetGroup)
            {
                targetGroup.Targets.Clear();
                targetGroup.Targets.Add(new CinemachineTargetGroup.Target()
                {
                    Object = _currentPossessed.transform,
                    Weight = 0.9f,
                    Radius = 2
                });
            }
            
            // Set possessed camera
            possessable.SetCamera(cam);
            
            // Invoke possessed event
            _currentPossessed.onPlayerPossess.Invoke();
        }
        
        public void OnMoveInput(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            onMove.Invoke(input);
        }
        
        public void OnJumpInput(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            onJump.Invoke();
        }

        public void OnAimInput(InputAction.CallbackContext context)
        {
            _noAimInput = false;
            _lastAimInput = context.ReadValue<Vector2>();
            _usingController = context.control.layout == CONTROLLER_LAYOUT_NAME;
        }

        public void OnPrimaryFireInput(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            onPrimaryFire.Invoke();
        }
        
        public void OnSecondaryFireInput(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            onSecondaryFire.Invoke();
        }

        public void OnInteractInput(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            onInteract.Invoke();
        }

        private void Update()
        {
            // We have no input, we need to update to aim in direction relative to controlled agent!
            // This is for controller input.
            if (_noAimInput)
            {
                Transform controlledAgentTransform = _currentPossessed.transform;
                Vector3 controlledAgentPos = controlledAgentTransform.transform.position;
                Vector3 aimPos = controlledAgentPos + controlledAgentTransform.forward * 8;
                onAim.Invoke(aimPos);
            }
            else
            {
                Vector3 aimAt;
                
                // Controller
                if (_usingController)
                {
                    // TODO: cast for enemies and set aim position to them!
                    // No input, we will aim towards controlled actor forward...
                    if (_lastAimInput == Vector2.zero) _noAimInput = true;
                    // Aim at direction of controller input
                    aimAt = _currentPossessed.transform.position + new Vector3(_lastAimInput.x, 0, _lastAimInput.y) * 16;
                }
                // Mouse & keyboard
                else
                {
                    // Aim at mouse world position
                    Physics.Raycast(cam.ScreenPointToRay(_lastAimInput), out RaycastHit hit, 1000);
                    aimAt = hit.point;
                }
                
                onAim.Invoke(aimAt);
            }
        }

        [Serializable]
        public class MoveEvent : UnityEvent<Vector2>
        {
        }

        [Serializable]
        public class AimEvent : UnityEvent<Vector3>
        {
        }
        
        [Serializable]
        public class FireEvent : UnityEvent
        {
        }

        [Serializable]
        public class InteractEvent : UnityEvent
        {
        }
        
        [Serializable]
        public class JumpEvent : UnityEvent
        {
        }
    }
}
