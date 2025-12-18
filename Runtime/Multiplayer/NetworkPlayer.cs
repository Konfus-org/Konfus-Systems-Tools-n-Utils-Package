using Armored_Felines.Input;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armored_Felines.Multiplayer
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField]
        private GameObject playerCamera;
        [SerializeField]
        private GameObject playerReticle;
        [SerializeField]
        private Rigidbody playerRb;
        
        private void Start()
        {
            if (IsOwner || UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                GetComponent<Player>().enabled = true;
                GetComponent<PlayerInput>().enabled = true;
                playerCamera.gameObject.SetActive(true);
                playerReticle.gameObject.SetActive(true);
            }
        }
    }
}