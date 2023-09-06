using UnityEngine;

namespace Konfus.Systems.AttribTable
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Konfus/Attribute Table/Attack Attribute")]
    public class Attack : ActorAttribute
    {
        public int damage;
        public float cooldownInSeconds;
    }
}