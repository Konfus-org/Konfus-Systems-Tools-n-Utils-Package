using Konfus.Utility.Custom_Types;
using UnityEngine;

namespace Konfus.Utility.Extensions
{
    public static class Vector3Extensions 
    {
        public static Vector3 RandomVector3(MinMaxFloat x, MinMaxFloat y, MinMaxFloat z)
        {
            return new Vector3(Random.Range(x.min, x.max), Random.Range(y.min, y.max), Random.Range(z.min, z.max));
        }
        
        public static void RotateAroundPivot(this ref Vector3 point, Vector3 pivot, Vector3 pivotAngles)
        {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(pivotAngles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
        }
        
        public static void Clamp(this Vector3 vector, float min, float max)
        {
            vector.x = Mathf.Clamp(vector.x, min, max);
            vector.y = Mathf.Clamp(vector.y, min, max);
            vector.z = Mathf.Clamp(vector.z, min, max);
        }
        
        public static void Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            vector.x = Mathf.Clamp(vector.x, min.x, max.x);
            vector.y = Mathf.Clamp(vector.y, min.y, max.y);
            vector.z = Mathf.Clamp(vector.z, min.z, max.z);
        }
        
        public static void Snap(this ref Vector3 v, float snapValue, float offset = 0)
        {
            var snappedVector = new Vector3
            (
                snapValue * Mathf.RoundToInt(v.x / snapValue) + offset,
                snapValue * Mathf.RoundToInt(v.y / snapValue) + offset,
                snapValue * Mathf.RoundToInt(v.z / snapValue) + offset
            );

            v = snappedVector;
        }
        
        public static bool IsInViewOfSceneCamera(this ref Vector3 position, float maxViewDist)
        {
            return position.IsInViewOfCamera(Camera.current, maxViewDist);
        }
        
        public static bool IsInViewOfCamera(this ref Vector3 position, Camera cam, float maxViewDist)
        {
            Vector3 cameraPos = cam.WorldToScreenPoint(position);
            return ((cameraPos.x >= 0) &&
                    (cameraPos.x <= cam.pixelWidth) &&
                    (cameraPos.y >= 0) &&
                    (cameraPos.y <= cam.pixelHeight) &&
                    (cameraPos.z > 0) &&
                    (cameraPos.z <= maxViewDist));
        }

        public static Vector3 ScreenToWorld(Camera camera, Vector3 position)
        {
            position.z = camera.nearClipPlane;
            return camera.ScreenToWorldPoint(position);
        }
        
        public static void DirFromAngle(this ref Vector3 v, float angleInDegrees)
        {
            v = new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

        public static float GetAngleOfVector(this ref Vector3 v)
        {
            v = v.normalized;
            float anlge = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            if (anlge < 0) anlge += 360;
            return anlge;
        }
        
        public static Vector3 StringToVector3(string sVector)
        {
            // Remove the parentheses
            if (sVector.StartsWith ("(") && sVector.EndsWith (")")) {
                sVector = sVector.Substring(1, sVector.Length-2);
            }
 
            // split the items
            string[] sArray = sVector.Split(',');
 
            // store as a Vector3
            Vector3 result = new Vector3(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]));
 
            return result;
        }
    }
    
    public static class Vector2Extensions 
    {
        public static Vector2 RotateAroundPivot(this Vector2 point, Vector2 pivot, Vector2 pivotAngles)
        {
            Vector2 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(pivotAngles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }
        
        public static Vector2 StringToVector2(string sVector)
        {
            // Remove the parentheses
            if (sVector.StartsWith ("(") && sVector.EndsWith (")")) {
                sVector = sVector.Substring(1, sVector.Length-2);
            }
 
            // split the items
            string[] sArray = sVector.Split(',');
 
            // store as a Vector3
            Vector2 result = new Vector2(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]));
 
            return result;
        }
    }
}
