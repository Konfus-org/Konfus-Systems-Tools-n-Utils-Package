using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Utility.Extensions
{
    public static class TransformExtensions
    {
        public static void Face(this Transform t, Transform target)
        {
            Vector3 dir = target.position - t.position;
            t.rotation = Quaternion.FromToRotation(target.transform.up, dir);
        }

        public static void Face(this Transform t, Vector3 target, Vector3 up)
        {
            Vector3 dir = target - t.position;
            t.rotation = Quaternion.FromToRotation(up, dir);
        }
        
        public static Quaternion GetFacingRotation(this Transform transform, Vector3 target, float maxDegreesDelta)
        {
            Vector3 targetDirection = (target - transform.position).normalized;
            Quaternion lookAtRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            return Quaternion.RotateTowards(transform.rotation, lookAtRotation, maxDegreesDelta);
        }

        public static void MoveTo(this Transform transform, Vector3 target, float maxMoveDelta)
        {
            Vector3 from = transform.position;
            Vector3 to = target;
            transform.position = Vector3.Lerp(from, to, maxMoveDelta);
        }
        
        public static bool IsFacing(this Transform t, Transform target)
        {
            return Vector3.Angle(t.forward, target.position - t.position) < 10;
        }

        public static Transform[] GetChildren(this Transform t)
        {
            int count = t.childCount;
            var children = new List<Transform>();
            for(int i = 0; i < count; i++)
            {
                Transform child = t.GetChild(i);
                children.Add(child);
            }

            return children.ToArray();
        }
    }
}