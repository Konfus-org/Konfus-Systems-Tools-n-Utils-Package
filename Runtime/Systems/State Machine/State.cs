using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Systems.Graph.Attributes;
using Konfus.Systems.Graph;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Systems.State_Machine
{
    [Serializable, Node("#9c0000", inputPortName = "From")]
    /*[CreateAssetMenu(fileName = "New State", menuName = "State Machine/New State")]*/
    public class State : INode /*: ScriptableObject*/
    {
        [SerializeField]
        private List<Action> actions;
        [PortList, SerializeReference]
        private List<Transition> transitions;

        public List<Transition> GetTransitions() => transitions;
        public void AddTransition(Transition transition) => transitions.Add(transition);
        public void ClearTransitions() => transitions.Clear();
        public List<Action> GetActions() => actions;
        public void AddAction(Action action) => actions.Add(action);
        public void ClearActions() => actions.Clear();
        
        public void OnEnter(StateEngine engine)
        {
            //controller.Agent.GetAnimator().Play(animationToPlay);
            foreach (Action action in actions)
                action.OnEnter(engine);
            foreach (Transition transition in transitions)
                transition.OnEnter(engine);
        }

        public void OnExit(StateEngine engine)
        {
            foreach (Action action in actions)
                action.OnExit(engine);
            foreach (Transition transition in transitions)
                transition.OnExit(engine);
        }

        public void OnUpdate(StateEngine engine)
        {
            foreach (Action action in actions)
                action.OnUpdate(engine);
        }

        public State CheckForTransition(StateEngine engine)
        {
            return transitions.Select(transition => transition.EvaluateCondition(engine) 
                ? this : transition.to).FirstOrDefault();
        }
    }

    [Serializable, InlineProperty, HideLabel]
    public class StateEvent
    {
        [SerializeField, FoldoutGroup("Events")]
        private UnityEvent enterStateEvent;
        [SerializeField, FoldoutGroup("Events")]
        private UnityEvent stateTickEvent;
        [SerializeField, FoldoutGroup("Events")]
        private UnityEvent exitStateEvent;

        public void TriggerEnterEvent() => enterStateEvent.Invoke();
        public void TriggerTickEvent() => stateTickEvent.Invoke();
        public void TriggerExitEvent() => exitStateEvent.Invoke();
        public void AddEnterListener(UnityAction action) => enterStateEvent.AddListener(action);
        public void AddTickListener(UnityAction action) => stateTickEvent.AddListener(action);
        public void AddExitListener(UnityAction action) => exitStateEvent.AddListener(action);
    }
}