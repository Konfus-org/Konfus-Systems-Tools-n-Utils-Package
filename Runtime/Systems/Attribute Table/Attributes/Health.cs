using Sirenix.OdinInspector;
using UnityEngine;

namespace Shuhari.Actors.Attributes
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Shuhari/Agents/Health Attribute")]
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