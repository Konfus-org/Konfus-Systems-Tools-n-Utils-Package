using Konfus.Pickups;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.PlayerInput
{
    public abstract class PlayerPossessable : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent onPlayerPossess;
        public UnityEvent onPlayerUnpossess;
        
        public bool IsPossessed { get; private set; }
        
        public Player PossessedBy { get; private set; }

        public void ClearPlayerPossession()
        {
            Player.Instance.PossessDefault();
        }

        public void Possess(Player player)
        {
            PossessedBy  = player;
            onPlayerPossess?.Invoke();
        }

        public void UnPossess()
        {
            PossessedBy = null;
            onPlayerUnpossess?.Invoke();
        }
        
        public abstract void Move(Vector2 input);
        public abstract void Aim(Vector3 target);
        public abstract void Interact();
        public abstract void PrimaryAction();
        public abstract void SecondaryAction();
        public abstract void Jump();
        public abstract void Crouch();
        public abstract bool Pickup(Pickup item);
        public abstract void Drop(Pickup item);

        private void Awake()
        {
            onPlayerPossess.AddListener(OnPossessed);
            onPlayerUnpossess.AddListener(OnUnpossessed);
        }

        private void OnPossessed()
        {
            IsPossessed = true;
        }

        private void OnUnpossessed()
        {
            IsPossessed = false;
        }
    }
}