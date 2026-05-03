using Konfus.Utility.Attributes;
using UnityEngine;

namespace Konfus.Input
{
    [Provide]
    [DisallowMultipleComponent]
    public sealed class DefaultPlayerPossessableProvider : MonoBehaviour
    {
        [SerializeField]
        private Possessable? possessable;

        public Possessable? Possessable => possessable;

        private void Reset()
        {
            possessable ??= GetComponent<Possessable>();
        }

        private void OnValidate()
        {
            possessable ??= GetComponent<Possessable>();
        }
    }
}
