using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Armored_Felines.Multiplayer
{
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkSpawner : NetworkBehaviour
    {
        public static NetworkSpawner Instance { get; private set; }
        
        private Dictionary<string, NetworkPrefab> _networkPrefabs;

        /// <summary>
        /// Spawns a client-side game object using the provided prefab, position, and rotation.
        /// This method sends a request to the server to spawn the object on all clients.
        /// </summary>
        /// <param name="goToSpawn">The prefab of the game object to spawn.</param>
        /// <param name="position">The position to spawn the game object at.</param>
        /// <param name="rotation">The rotation to spawn the game object with.</param>
        /// <returns>The spawned game object.</returns>
        public GameObject SpawnClientSide(GameObject goToSpawn, Vector3 position, Quaternion rotation)
        {
            var spawnedObj = SpawnObj(goToSpawn.name, position, rotation);
            RequestSpawnServerRpc(goToSpawn.name, position, rotation, spawnOnServer: false);
            return spawnedObj;
        }

        /// <summary>
        /// Spawns a server-side game object using the provided prefab, position, and rotation.
        /// Sends a request to the server to spawn objects on the server which is replicated to all clients.
        /// </summary>
        /// <param name="goToSpawn">The prefab of the game object to spawn. Must be in one of the provided NetworkPrefabsList.</param>
        /// <param name="position">The position to spawn the game object at.</param>
        /// <param name="rotation">The rotation to spawn the game object with.</param>
        public void SpawnServerSide(GameObject goToSpawn, Vector3 position, Quaternion rotation)
        {
            RequestSpawnServerRpc(goToSpawn.name, position, rotation, spawnOnServer: true);
        }
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            _networkPrefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.NetworkPrefabsLists
                .SelectMany(list => list.PrefabList.Select(prefab => prefab)).ToDictionary(key => key.Prefab.name);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RequestSpawnServerRpc(string prefabName, Vector3 position, Quaternion rotation, bool spawnOnServer, ServerRpcParams rpcParams = default)
        {
            if (spawnOnServer)
            {
                var spawnedNetworkObj = SpawnObj(prefabName, position, rotation).GetComponent<NetworkObject>();
                spawnedNetworkObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
                NotifyClientOfSpawnOnServerClientRpc(spawnedNetworkObj.NetworkObjectId, rpcParams.Receive.SenderClientId);
            }
            else
            {
                SpawnOnClientsClientRpc(prefabName, position, rotation, rpcParams.Receive.SenderClientId);
            }
        }
        
        [ClientRpc]
        private void SpawnOnClientsClientRpc(string prefabName, Vector3 position, Quaternion rotation, ulong requestingClientId)
        {
            if (NetworkManager.Singleton.LocalClientId == requestingClientId) return;
            var spawnedObj = SpawnObj(prefabName, position, rotation);
            Debug.Log($"Spawned Object: {spawnedObj.name}");
        }

        [ClientRpc]
        private void NotifyClientOfSpawnOnServerClientRpc(ulong networkObjectId, ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                var spawnedObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].gameObject;
                Debug.Log($"Spawned Object On Server: {spawnedObject.name}");
            }
        }

        private GameObject SpawnObj(string prefabName, Vector3 position, Quaternion rotation)
        {
            if (!_networkPrefabs.TryGetValue(prefabName, out NetworkPrefab networkPrefab))
            {
                Debug.LogError($"Could not find prefab with name {prefabName}! Ensure the prefab has been added to the NetworkManager's Prefabs list.");
                return null;
            }
            
            GameObject prefab = networkPrefab.Prefab;
            GameObject networkObjectToSpawn = NetworkManager.Singleton.GetNetworkPrefabOverride(prefab);
            return Instantiate(networkObjectToSpawn, position, rotation);
        }
    }
}
