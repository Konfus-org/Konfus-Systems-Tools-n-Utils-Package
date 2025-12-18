using UnityEngine;

namespace Konfus.Blackboard.Variables
{
    public class BlackboardVec3 : BlackboardVar<Vector3>
    {
        public BlackboardVec3() : base() { }
        public BlackboardVec3(Vector3 val) : base(val) { }
    }
}