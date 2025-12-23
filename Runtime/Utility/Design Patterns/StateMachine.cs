using System;
using System.Collections.Generic;
using System.Linq;

namespace Konfus.Utility.Design_Patterns
{
    public abstract class State
    {
        protected State(string name)
        {
            Name = name;
        }

        public string Name { get; protected set; }
        public bool IsPlaying { get; internal set; }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void Tick()
        {
        }
    }

    public class StateMachine
    {
        private static readonly List<Transition> EmptyTransitions = new(0);
        private readonly List<Transition> _anyTransitions = new();
        private readonly Dictionary<string, List<Transition>> _transitions = new();
        private State? _currentState;

        private List<Transition> _currentTransitions = new();
        private State? _lastState;

        public void Tick()
        {
            var transition = GetTransition();
            if (transition != null)
                SetState(transition.To);

            _currentState?.Tick();
        }

        public State? GetLastState()
        {
            return _lastState;
        }

        public State? GetCurrentState()
        {
            return _currentState;
        }

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
            if (!_transitions.TryGetValue(from.Name, out var transitions))
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

        private Transition? GetTransition()
        {
            foreach (var transition in _anyTransitions.Where(transition => transition.Condition()))
            {
                return transition;
            }

            return _currentTransitions.FirstOrDefault(transition => transition.Condition());
        }

        private class Transition
        {
            public Transition(State to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }

            public Func<bool> Condition { get; }
            public State To { get; }
        }
    }
}