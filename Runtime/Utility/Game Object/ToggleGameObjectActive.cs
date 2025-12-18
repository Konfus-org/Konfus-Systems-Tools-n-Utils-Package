using UnityEngine;

namespace Armored_Felines.Utility
{
    public class ToggleGameObjectActive : MonoBehaviour
    {
        public GameObject target;
        
        public void Toggle()
        {
            target.SetActive(!target.activeSelf);
        }
    }
}
