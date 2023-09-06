using UnityEngine;

namespace Konfus.Systems.AttribTable
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Konfus/Attribute Table/Locomotion Attribute")]
    public class Locomotion : ActorAttribute
    {
        public float acceleration;
        public float rotationSpeed;
        public float moveSpeed;
    }
}