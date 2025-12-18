using Unity.Collections;
using UnityEngine;

namespace Konfus.Attribute_Table
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Konfus/Attribute Table/Health Attribute")]
    public class Health : Attribute
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