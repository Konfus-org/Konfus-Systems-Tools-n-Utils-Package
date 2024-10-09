using Konfus.Utility.Extensions;
using UnityEngine;

namespace Konfus.Systems.Oscillators
{
    /// <summary>
    ///     Limits the range of rotation of this rigid body.
    /// </summary>
    public class LimitRotation : MonoBehaviour
    {
        [SerializeField, Tooltip("+- Range of rotations for each respective axis.")] 
        private Vector3 maxLocalRotation = Vector3.one * 360f;
        private Rigidbody _rb;

        /// <summary>
        ///     Define the rigid body.
        /// </summary>
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        ///     Clamp the rotation to be less than the desired maxLocalRotation.
        /// </summary>
        private void FixedUpdate()
        {
            Quaternion clampedLocalRot = transform.localRotation.Clamp(maxLocalRotation);
            _rb.MoveRotation(clampedLocalRot);
        }
    }
}