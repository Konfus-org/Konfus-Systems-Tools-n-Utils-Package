using UnityEngine;

namespace Konfus.Attribute_Table
{
    [CreateAssetMenu(fileName = "New Attribute", menuName = "Konfus/Attribute Table/Locomotion Attribute")]
    public class Locomotion : Attribute
    {
        public float acceleration;
        public float rotationSpeed;
        public float moveSpeed;
    }
}