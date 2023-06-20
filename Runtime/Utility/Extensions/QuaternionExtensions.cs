using UnityEngine;

namespace Konfus.Utility.Extensions
{
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Calculates the multiplication of a Quaternion by a scalar, such as to alter the scale of a rotation.
        /// </summary>
        /// <param name="input">The Quaternion rotation to be scaled.</param>
        /// <param name="scalar">The scale by which to multiply the rotation.</param>
        /// <returns>The scale adjusted Quaternion.</returns>
        public static Quaternion Multiply(this Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
        
        /// <summary>
        /// Calculates the shortest rotation between two Quaternions.
        /// </summary>
        /// <param name="a">The first Quaternion, from which the rotation is to be calculated.</param>
        /// <param name="b">The second Quaternion, for which the rotation is the goal.</param>
        /// <returns>The shortest rotation from a to b.</returns>
        public static Quaternion CalculateShortestRotationTo(this Quaternion a, Quaternion b)
        { 
            if (Quaternion.Dot(a, b) < 0)
            {
                return a * Quaternion.Inverse(Multiply(b, -1));
            }

            else return a * Quaternion.Inverse(b);
        }
        
        /// <summary>
        /// Clamps the axes of a Quaternion to be between a minimum and a maximum.
        /// </summary>
        /// <param name="q">The unclamped Quaternion.</param>
        /// <param name="bounds">A Vector3 representing absolute range of the Euler rotation for clamping.</param>
        /// <returns>The axes-clamped Quaternion.</returns>
        public static Quaternion Clamp(this Quaternion q, Vector3 bounds)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, -bounds.x, bounds.x);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
            angleY = Mathf.Clamp(angleY, -bounds.y, bounds.y);
            q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);

            float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
            angleZ = Mathf.Clamp(angleZ, -bounds.z, bounds.z);
            q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);

            return q.normalized;
        }
    }
}