using UnityEngine;

namespace Konfus.Blackboard
{
    public class BlackboardContainer : MonoBehaviour
    {
        [SerializeField]
        private Blackboard? blackboard;
        public Blackboard? Blackboard => blackboard;
    }
}