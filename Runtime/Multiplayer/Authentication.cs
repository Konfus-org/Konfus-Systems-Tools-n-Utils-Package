using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Armored_Felines.Multiplayer
{
    public static class Authentication
    {
        public static string PlayerId { get; private set; }

        public static async Task LoginAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                Debug.Log("Initializing Unity Services....");
                var options = new InitializationOptions();
                
/*#if UNITY_EDITOR
                // NOTE: Removed since we are no longer using parrel sync for multiplayer dev, this sort of system is now built into Unity!
                // kept this around in case we have to do something similar with the new system...
                options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif*/

                await UnityServices.InitializeAsync(options);
                Debug.Log("Finished initializing Unity Services!");
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Signing player in....");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                PlayerId = AuthenticationService.Instance.PlayerId;
                Debug.Log($"Player {PlayerId} has signed in!");
            }
        }
    }
}