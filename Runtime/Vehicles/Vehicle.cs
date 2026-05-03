using Konfus.Input;
using UnityEngine;

namespace Konfus.Vehicles
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Possessable))]
    public class Vehicle : MonoBehaviour
    {
        [SerializeField]
        private Possessable? possessable;

        public Possessable? Possessable => possessable;

        private void Awake()
        {
            possessable ??= GetComponent<Possessable>();
        }

        public bool Possess(Possessable possessor)
        {
            return possessable != null && possessor != null && possessor.Possess(possessable);
        }

        public bool PossessFromInteractor(Interactor interactor)
        {
            return possessable != null && possessable.PossessFromInteractor(interactor);
        }

        public bool Unpossess()
        {
            return possessable != null && possessable.Unpossess();
        }

        public void Move(Vector2 input)
        {
            possessable?.Move(input);
        }

        public void Look(Vector2 input)
        {
            possessable?.Look(input);
        }

        public void StartJump()
        {
            possessable?.StartJump();
        }

        public void StopJump()
        {
            possessable?.StopJump();
        }

        public void StartSprint()
        {
            possessable?.StartSprint();
        }

        public void StopSprint()
        {
            possessable?.StopSprint();
        }

        public void Interact()
        {
            possessable?.Interact();
        }
    }
}
