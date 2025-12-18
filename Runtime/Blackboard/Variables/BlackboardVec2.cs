using UnityEngine;

namespace Konfus.Blackboard.Variables
{
    public class BlackboardVec2 : BlackboardVar<Vector2>
    {
        public BlackboardVec2() : base() { }
        public BlackboardVec2(Vector2 val) : base(val) { }
    }
}