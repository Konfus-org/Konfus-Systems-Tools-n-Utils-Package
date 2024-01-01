using Unity.Collections;
using UnityEngine;

namespace Konfus.Systems.AttribTable
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Konfus/Attribute Table/Health Attribute")]
    public class Health : ActorAttribute
    {
        public int max;
        [ReadOnly] 
        public int current;
        public bool invulnerable;

        private void OnValidate()
        {
            current = max;
        }
    }
}