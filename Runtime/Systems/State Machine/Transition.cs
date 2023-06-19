using System;

namespace Konfus.Systems.State_Machine
{
    [Serializable]
    public class Transition
    {
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