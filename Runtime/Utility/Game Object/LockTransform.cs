using UnityEngine;

namespace Konfus.Utility.Game_Object
{
    public class LockTransform : MonoBehaviour
    {
        [Header("Rotation:")]
        [SerializeField]
        private bool lockXRotation;
        [SerializeField]
        private bool lockYRotation;
        [SerializeField]
        private bool lockZRotation;

        [Header("Position:")]
        [SerializeField]
        private bool lockXPosition;
        [SerializeField]
        private bool lockYPosition;
        [SerializeField]
        private bool lockZPosition;

        [Header("Scale:")]
        [SerializeField]
        private bool lockXScale;
        [SerializeField]
        private bool lockYScale;
        [SerializeField]
        private bool lockZScale;

        private void LateUpdate()
        {
            // Rotation
            if (lockXRotation)
                transform.localRotation = Quaternion.Euler(0, transform.localRotation.y, transform.localRotation.z);
            if (lockYRotation)
                transform.localRotation = Quaternion.Euler(transform.localRotation.x, 0, transform.localRotation.z);
            if (lockZRotation)
                transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, 0);

            // Position
            if (lockXPosition)
                transform.localPosition = new Vector3(0, transform.localPosition.y, transform.localPosition.z);
            if (lockYPosition)
                transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
            if (lockZPosition)
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);

            // Scale
            if (lockXScale) transform.localScale = new Vector3(0, transform.localScale.y, transform.localScale.z);
            if (lockYScale) transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
            if (lockZScale) transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 0);
        }
    }
}