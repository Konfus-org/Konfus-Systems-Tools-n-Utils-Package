using System;
using UnityEngine;

namespace Konfus.Blackboard.Variables
{
    public abstract class BlackboardVar
    {
    }

    [Serializable]
    public class BlackboardVar<T> : BlackboardVar
    {
        [SerializeField]
        private T? value;

        public BlackboardVar()
        {
            value = default;
        }

        public BlackboardVar(T val)
        {
            value = val;
        }

        public T? Value()
        {
            return value;
        }

        public void Set(T newValue)
        {
            value = newValue;
        }
    }
}