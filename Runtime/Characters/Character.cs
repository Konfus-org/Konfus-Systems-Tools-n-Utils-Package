using Konfus.PlayerInput;
using Konfus.Pickups;
using UnityEngine;

namespace Konfus.Characters
{
    public abstract class Character : PlayerPossessable
    {
        [SerializeField]
        protected Interactor interactor;
        
        public override void Interact()
        {
            interactor.TryToInteract();
        }
        
        public override void Move(Vector2 input) {}
        public override void Jump() {}
        public override void Crouch() {}
        public override bool Pickup(Pickup item) => false;
        public override void Drop(Pickup item) {}
        public override void Aim(Vector3 target) {}
        public override void PrimaryAction() {}
        public override void SecondaryAction() {}
    }
}