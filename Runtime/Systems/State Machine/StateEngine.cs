using Konfus.Utility.Serialization;
using UnityEngine;

namespace Konfus.Systems.State_Machine
{
    public class StateEngine : MonoBehaviour
    {
        /*public SerializableDict<string, StateEvent> stateEvents;*/

        [SerializeField]
        private StateMachine stateMachine;

        private void Start()
        {
            stateMachine.OnStart(this);
        }

        private void Update()
        {
            stateMachine.OnUpdate(this);
        }
    }
}