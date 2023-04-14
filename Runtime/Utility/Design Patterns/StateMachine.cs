using System;
using System.Collections.Generic;

namespace Konfus.Utility.Design_Patterns
{
    public abstract class State
    {
        public string Name { get; protected set; }
        public bool IsPlaying { get; internal set; }

        public State(string name)
        {
            Name = name;
        }
        
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void Tick() { }
    }
    
    public class StateMachine
    {
        private State _lastState;
        private State _currentState;
        private readonly Dictionary<String, List<Transition>> _transitions = new Dictionary<String, List<Transition>>();

        private List<Transition> _currentTransitions = new List<Transition>();
        private readonly List<Transition> _anyTransitions = new List<Transition>();

        private static readonly List<Transition> EmptyTransitions = new List<Transition>(0);

        public void Tick()
        {
            Transition transition = GetTransition();
            if (transition != null)
                SetState(transition.To);

            _currentState?.Tick();
        }

        public State GetLastState() => _lastState;
        public State GetCurrentState() => _currentState;
        
        public void SetState(State state)
        {
            if (state == _currentState)
                return;

            if (_currentState != null)
            {
                _currentState.IsPlaying = false;
                _currentState.OnExit();
                _lastState = _currentState;
            }
            
            _currentState = state;

            _transitions.TryGetValue(_currentState.Name, out _currentTransitions);
            _currentTransitions ??= EmptyTransitions;
            
            _currentState.IsPlaying = true;
            _currentState.OnEnter();
        }

        public void AddTransition(State from, State to, Func<bool> predicate)
        {
            if (_transitions.TryGetValue(from.Name, out List<Transition> transitions) == false)
            {
                transitions = new List<Transition>();
                _transitions[from.Name] = transitions;
            }

            transitions.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(State state, Func<bool> predicate)
        {
            _anyTransitions.Add(new Transition(state, predicate));
        }

        private class Transition
        {
            public Func<bool> Condition { get; }
            public State To { get; }

            public Transition(State to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        private Transition GetTransition()
        {
            foreach (Transition transition in _anyTransitions)
                if (transition.Condition())
                    return transition;

            foreach (Transition transition in _currentTransitions)
                if (transition.Condition())
                    return transition;

            return null;
        }
    }
}