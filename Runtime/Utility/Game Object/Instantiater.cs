using Konfus.Multiplayer;
using Unity.Services.Core;
using UnityEngine;

namespace Konfus.Utility.Game_Object
{
    public static class Instantiater
    {
        public static GameObject Instantiate(GameObject gameObject, Vector3 position, Quaternion rotation)
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                NetworkSpawner.Instance.SpawnClientSide(gameObject, position, rotation);
            }
            else
            {
                return Object.Instantiate(gameObject, position, rotation);
            }

            return null;
        }
    }
}