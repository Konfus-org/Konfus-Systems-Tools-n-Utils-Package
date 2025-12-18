using Konfus.Characters;
using Konfus.PlayerInput;
using UnityEngine;

namespace Konfus.Pickups
{
    public class Pickup : MonoBehaviour
    {
        public void OnPickup(Interactor interactor)
        {
            var interactingCharacter = interactor.gameObject.GetComponent<Character>();
            if (interactingCharacter)
                interactingCharacter.Pickup(this);
            else
                Debug.LogWarning("Only characters can use pickups right now!");
        }
    }
}
