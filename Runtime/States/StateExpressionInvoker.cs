using UnityEngine;

namespace Konfus.States
{
    public class StateExpressionInvoker : MonoBehaviour
    {
        [SerializeField] private StateController? stateController;
        [SerializeField] private StateExpression expression = new();

        public StateController? StateController => stateController;
        public StateExpression Expression => expression;

        public void Invoke()
        {
            if (stateController == null)
            {
                Debug.LogWarning($"No {nameof(StateController)} assigned on '{name}'.", this);
                return;
            }

            stateController.EvaluateAndChangeState(expression);
        }
    }
}
