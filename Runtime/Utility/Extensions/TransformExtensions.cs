using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Utility.Extensions
{
    public static class TransformExtensions
    {
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