using UnityEngine;

namespace Konfus.Systems.Blackboard.Variables
{
    public class BlackboardVec3 : BlackboardVar<Vector3>
    {
        public BlackboardVec3() : base() { }
        public BlackboardVec3(Vector3 val) : base(val) { }
    }
}