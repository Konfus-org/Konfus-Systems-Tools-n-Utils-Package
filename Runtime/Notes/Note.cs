using UnityEngine;

namespace Konfus.Notes
{
    public class Note : MonoBehaviour
    {
        // Normally I wouldn't do something dirty like this...
        // but in this case we don't want any notes compiled out when building the game.
#if UNITY_EDITOR
        [SerializeField]
        private string text = "";

        public string Text
        {
            get => text;
            internal set => text = value;
        }
#endif
    }
}