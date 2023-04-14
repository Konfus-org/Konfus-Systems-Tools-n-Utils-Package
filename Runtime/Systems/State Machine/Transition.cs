using System;
using Konfus.Systems.Graph;
using Konfus.Systems.Graph.Attributes;
using UnityEngine;

namespace Konfus.Systems.State_Machine
{
    [Serializable, Node("#007bbd", inputPortName = "From")]
    public class Transition : INode
    {
        [Port("To"), SerializeReference]
        public State to;
        
        public Condition condition;

        public void OnEnter(StateEngine engine)
        {
            condition.OnEnter(engine);
        }

        public void OnExit(StateEngine engine)
        {
            condition.OnExit(engine);
        }

        public bool EvaluateCondition(StateEngine engine)
        {
            return condition == null || condition.Evaluate(engine);
        }
    }
}