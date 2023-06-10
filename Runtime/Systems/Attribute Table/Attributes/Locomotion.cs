using UnityEngine;

namespace Shuhari.Actors.Attributes
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Shuhari/Agents/Locomotion Attribute")]
    public class Locomotion : ActorAttribute
    {
        public float acceleration;
        public float rotationSpeed;
        public float moveSpeed;
    }
}