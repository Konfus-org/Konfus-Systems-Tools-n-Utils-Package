using UnityEngine;
using UnityEngine.Events;
using Konfus.Utility.Attributes;

namespace Konfus.Input
{
    public class Interactable : MonoBehaviour
    {
        public UnityEvent<Interactor>? onInteractedWith;

        [ReadOnly]
        public Interactor? lastIneractor;

        public void Interact(Interactor interactor)
        {
            onInteractedWith?.Invoke(interactor);
            lastIneractor = interactor;
        }
    }
}
