using UnityEngine;

namespace Konfus.Systems.Blackboard.Variables
{
    public class BlackboardTransform : BlackboardVar<Transform>
    {
        public BlackboardTransform() : base() { }
        public BlackboardTransform(Transform val) : base(val) { }
    }
}