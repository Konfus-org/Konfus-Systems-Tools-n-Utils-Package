using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.State_Machine
{
    //[InlineEditor, CreateAssetMenu(fileName = "New Condition", menuName = "State Machine/New State Machine")]
    public abstract class Condition : ScriptableObject
    {
        public abstract void OnEnter(StateEngine engine);
        public abstract bool Evaluate(StateEngine engine);
        public abstract void OnExit(StateEngine engine);
    }
}