using UnityEngine;

namespace Konfus.Systems.Notes
{
    public class Note : MonoBehaviour
    {
        [SerializeField]
        private string text;

        public string Text
        {
            get => text;
            internal set => text = value;
        }
    }
}