using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.State_Machine
{
    [InlineEditor, CreateAssetMenu(fileName = "New Action", menuName = "Konfus/State Machine/New State Machine")]
    public abstract class Action : ScriptableObject
    {
        public abstract void OnEnter(StateEngine engine);
        public abstract void OnUpdate(StateEngine engine);
        public abstract void OnExit(StateEngine engine);
    }
}