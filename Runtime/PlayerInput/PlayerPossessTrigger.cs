using UnityEngine;

namespace Konfus.PlayerInput
{
    [RequireComponent(typeof(BoxCollider))]
    public class PlayerPossessTrigger : MonoBehaviour
    {
        [SerializeField]
        private PlayerPossessable controllable;
        
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<Player>();
            player?.Possess(controllable);
        }
    }
}
