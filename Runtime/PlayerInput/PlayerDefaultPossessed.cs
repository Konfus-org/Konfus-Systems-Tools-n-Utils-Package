using Unity.Netcode;
using Unity.Services.Core;

namespace Konfus.PlayerInput
{
    public class PlayerDefaultPossessed : NetworkBehaviour
    {
        private void Start()
        {
            if (IsOwner || UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                Player.Instance.SetDefaultPossessed(GetComponent<PlayerPossessable>());
            }
        }
    }
}