using UnityEngine;

namespace Konfus.Utility.Game_Object
{
    public class ToggleGameObjectActive : MonoBehaviour
    {
        [SerializeField]
        private GameObject? target;

        private void Start()
        {
            if (!target) target = gameObject;
        }

        public void Toggle()
        {
            target?.SetActive(!target.activeSelf);
        }
    }
}