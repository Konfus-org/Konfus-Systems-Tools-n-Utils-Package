using System.Diagnostics;
using Konfus.Systems.Graph;
using UnityEditor;
using UnityEngine;

namespace Konfus.Systems.State_Machine
{
    [CreateAssetMenu(fileName = "New State Machine", menuName = "Konfus/State Machine/New State Machine", order = 1)]
    public class StateMachine : Graph.Graph
    {
        [SerializeField, HideInInspector] private bool _hasBeenInitialised = false;

        private StartingState _startingState;
        private State _currentState;
        
        public State GetCurrentState() => _currentState;
        
        public void OnStart(StateEngine engine)
        {
            _currentState = _startingState.startAt;
            _currentState.OnEnter(engine);
        }
        
        public void OnUpdate(StateEngine engine)
        {
            /*if (engine.stateEvents.ContainsKey(_currentState.name))
                engine.stateEvents[_currentState.name].TriggerTickEvent();*/
            
            _currentState.OnUpdate(engine);
            
            State transition = _currentState.CheckForTransition(engine);
            if (transition != _currentState)
                ChangeState(transition, engine);
        }

        public void ChangeState(State nextState, StateEngine engine)
        {
            /*if (engine.stateEvents.ContainsKey(_currentState.name))
                engine.stateEvents[_currentState.name].TriggerExitEvent();*/
            
            _currentState.OnExit(engine);
            _currentState = nextState;
            
            /*if (engine.stateEvents.ContainsKey(_currentState.name))
                engine.stateEvents[_currentState.name].TriggerEnterEvent();*/
            
            _currentState.OnEnter(engine);
        }
        
        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            if (_hasBeenInitialised) return;
            
            _hasBeenInitialised = true;
            CreateSerializedObject();
            AddNode(new Node(new StartingState()));
            ForceSerializationUpdate();
            AssetDatabase.SaveAssets();
        }
    }
}