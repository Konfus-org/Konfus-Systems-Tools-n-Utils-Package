using UnityEngine;

namespace Shuhari.Actors.Attributes
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Shuhari/Agents/Attack Attribute")]
    public class Attack : ActorAttribute
    {
        public int damage;
        public float cooldownInSeconds;
    }
}