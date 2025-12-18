using Konfus.Characters;
using Konfus.PlayerInput;
using UnityEngine;

namespace Konfus.Pickups
{
    public class Pickup : MonoBehaviour
    {
        [SerializeField] protected string activeName;
        public void OnPickup(Interactor interactor)
        {
            var interactingCharacter = interactor.gameObject.GetComponent<Character>();
            if (interactingCharacter)
            {
                interactingCharacter.HandlePickup(this);
            }
            else
            {
                Debug.LogWarning("Only characters can use pickups right now!");
            }
        }

        public string GetActiveName()
        {
            return activeName;
        }
    }
}
