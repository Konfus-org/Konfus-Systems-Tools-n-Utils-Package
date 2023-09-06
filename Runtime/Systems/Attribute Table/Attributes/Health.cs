using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.AttribTable
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Konfus/Attribute Table/Health Attribute")]
    public class Health : ActorAttribute
    {
        [OnValueChanged("OnMaxValChanged", InvokeOnInitialize = true)]
        public int max;
        [ReadOnly] 
        public int current;
        public bool invulnerable;
        
        private void OnMaxValChanged() => current = max;
    }
}