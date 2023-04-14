using UnityEngine;

namespace Konfus.Systems.Blackboard
{
    public class BlackboardContainer : MonoBehaviour
    {
        [SerializeField]
        private Blackboard blackboard;
        public Blackboard Blackboard => blackboard;
    }
}