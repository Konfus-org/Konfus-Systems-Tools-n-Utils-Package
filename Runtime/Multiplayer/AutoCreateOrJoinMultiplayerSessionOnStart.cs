using System;
using Konfus.Utility.Extensions;
using UnityEngine;

namespace Armored_Felines.Multiplayer
{
    public class AutoCreateOrJoinMultiplayerSessionOnStart : MonoBehaviour
    {
        [SerializeField]
        private int maxPlayers = 4;

        private async void Start()
        {
            try
            {
                await Authentication.LoginAsync().ContinueOnSameContext();

                if (!await MatchmakingService.QuickJoinLobbyAsync())
                {
                    await MatchmakingService.CreateSessionAsync(Authentication.PlayerId, maxPlayers).ContinueOnSameContext();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to auto create or join a lobby! Exception: {e}");
            }
        }
    }
}