using Unity.Mathematics;
using UnityEngine;

namespace Konfus.Utility.Extensions
{
    public static class Float3Extensions
    {
        public static Vector3 ToVector3(this float3 f3)
        {
            return new Vector3(f3.x, f3.y, f3.z);
        }
        
        public static Vector2 ToVector2(this float3 f3)
        {
            return f3.ToVector3();
        }
    }
}