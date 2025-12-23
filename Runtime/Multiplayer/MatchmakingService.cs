using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Konfus.Utility.Extensions;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace Konfus.Multiplayer
{
    public static class MatchmakingService
    {
        private const string JoinCodeKey = "joinCode";
        private const string PlayerNameKey = "playerName";

        public static ISession? ActiveSession { get; private set; }

        public static event Action<Lobby> CurrentLobbyRefreshed;

        public static async Task<Dictionary<string, PlayerProperty>> GetPlayerProperties()
        {
            string? playerName = await AuthenticationService.Instance.GetPlayerNameAsync().ContinueOnSameContext();
            var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
            return new Dictionary<string, PlayerProperty> { { PlayerNameKey, playerNameProperty } };
        }

        public static async Task CreateSessionAsync(string name, int maxPlayers)
        {
            Dictionary<string, PlayerProperty>? playerProperties = await GetPlayerProperties().ContinueOnSameContext();
            SessionOptions sessionOptions = new SessionOptions
            {
                MaxPlayers = maxPlayers,
                IsLocked = false,
                IsPrivate = false,
                PlayerProperties = playerProperties
            }.WithRelayNetwork();
            ActiveSession =
                await MultiplayerService.Instance.CreateSessionAsync(sessionOptions).ContinueOnSameContext();

            Debug.Log($"Created lobby: {name}, max players: {maxPlayers}");
        }

        public static async Task JoinSessionByIdAsync(string sessionId)
        {
            ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
            Debug.Log($"Session {ActiveSession.Id} joined!");
        }

        public static async Task JoinSessionByCode(string sessionCode)
        {
            ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
            Debug.Log($"Session {ActiveSession.Id} joined!");
        }

        public static async Task<bool> QuickJoinLobbyAsync()
        {
            IList<ISessionInfo>? availableSessions = await QuerySessionsAsync().ContinueOnSameContext();
            if (availableSessions.IsNullOrEmpty())
            {
                Debug.Log("No sessions available to join...");
                return false;
            }

            await JoinSessionByIdAsync(availableSessions.First().Id).ContinueOnSameContext();
            return true;
        }

        public static async Task<IList<ISessionInfo>> QuerySessionsAsync()
        {
            var sessionQueryOptions = new QuerySessionsOptions();
            QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
            return results.Sessions;
        }

        public static async Task KickPlayerAsync(string playerId)
        {
            if (!ActiveSession.IsHost) return;
            await ActiveSession.AsHost().RemovePlayerAsync(playerId);
        }

        public static async Task LeaveSessionAsync()
        {
            if (ActiveSession != null)
                try
                {
                    await ActiveSession.LeaveAsync();
                }
                catch
                {
                    // Ignored as we are exiting the game
                }
                finally
                {
                    ActiveSession = null;
                }
        }
    }
}