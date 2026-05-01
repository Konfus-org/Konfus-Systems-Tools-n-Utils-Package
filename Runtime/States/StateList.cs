using System.Collections.Generic;
using UnityEngine;

namespace Konfus.States
{
    [CreateAssetMenu(fileName = "NewStateList", menuName = "State Machine/State List")]
    public class StateList : ScriptableObject
    {
        // This list can be edited directly in the Project window
        public List<string> availableStates = new List<string>();
    }
}