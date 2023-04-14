using UnityEngine;

namespace Konfus.Code.Scripts.Konfus.Systems.AI
{
    public class Brain : MonoBehaviour, IBrain
    {
        public IAgent ControlledAgent { get; protected set; }
    }
}