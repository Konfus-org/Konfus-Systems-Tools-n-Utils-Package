using System;
using UnityEngine;

namespace Konfus.Systems.State_Machine
{
    [Serializable]
    public class StartingState
    {
        [SerializeField]
        public State startAt;
    }
}