using UnityEngine;

namespace Konfus.Utility.Game_Object
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
