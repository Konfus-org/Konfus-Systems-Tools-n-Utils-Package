using UnityEditor;
using UnityEngine;

namespace Konfus.Utility.Extensions.Editor
{
    public static class HandlesExtensions
    {
        public static void DrawWireCapsule(Vector3 center, float radius, float height)
        {
            float pointOffset = (height - (radius * 2)) / 2;
 
            //draw sideways
            Handles.DrawWireArc(center + Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, radius);
            Handles.DrawLine(center + new Vector3(0, pointOffset, -radius), center + new Vector3(0, -pointOffset, -radius));
            Handles.DrawLine(center + new Vector3(0, pointOffset, radius), center + new Vector3(0, -pointOffset, radius));
            Handles.DrawWireArc(center + Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, radius);
            //draw frontways
            Handles.DrawWireArc(center + Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, radius);
            Handles.DrawLine(center + new Vector3(-radius, pointOffset, 0), center + new Vector3(-radius, -pointOffset, 0));
            Handles.DrawLine(center + new Vector3(radius, pointOffset, 0), center + new Vector3(radius, -pointOffset, 0));
            Handles.DrawWireArc(center + Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, radius);
            //draw center
            Handles.DrawWireDisc(center + Vector3.up * pointOffset, Vector3.up, radius);
            Handles.DrawWireDisc(center + Vector3.down * pointOffset, Vector3.up, radius);
        }
    }
}