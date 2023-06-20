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