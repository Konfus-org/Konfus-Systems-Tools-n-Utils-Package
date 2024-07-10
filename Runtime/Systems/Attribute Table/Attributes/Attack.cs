using UnityEngine;

namespace Konfus.Systems.Attribute_Table
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Konfus/Attribute Table/Attack Attribute")]
    public class Attack : Attribute
    {
        public int damage;
        public float cooldownInSeconds;
    }
}