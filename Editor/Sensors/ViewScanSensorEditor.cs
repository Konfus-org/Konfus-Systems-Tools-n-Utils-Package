using System.Collections.Generic;
using Konfus.Systems.Sensor_Toolkit;
using Konfus.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Sensors
{
    [CustomEditor(typeof(ViewScanSensor))]
    public class ViewScanSensorEditor : SensorEditor
    {
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void DrawGizmos(ViewScanSensor sensor, GizmoType gizmoType)
        {
            sensor.Scan();
            DrawSensor(sensor);
        }
        
        private static void DrawSensor(ViewScanSensor sensor)
        {
            // If we have hits draw line to what was detected
            if (!sensor.hits.IsNullOrEmpty())
            {
                foreach (Sensor.Hit hitInfo in sensor.hits)
                {
                    Gizmos.color = SensorColors.NoHitColor;
                    Gizmos.DrawLine(sensor.transform.position, hitInfo.point);
                }
            }
            
            // Draw FOV mesh and inevitable detection mesh
            Gizmos.color = SensorColors.NoHitColor;
            if (!sensor.hits.IsNullOrEmpty()) Gizmos.color = SensorColors.HitColor;
            
            Gizmos.matrix = Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);
            Gizmos.DrawMesh(CreateFovGizmoMesh(sensor.transform, sensor.ObstructionFilter, sensor.Fov, sensor.SensorLength));
            
            // SENSOR CORNER DETECTION DEBUG CODE
            /*{
                var transform = sensor.transform;
                RaycastHit[] spherecastHits = Physics.SphereCastAll(transform.position, sensor.SensorLength,
                    transform.forward, 0.001f, sensor.DetectionFilter);
                // Remove hits not within sight
                foreach (RaycastHit hitInfo in spherecastHits)
                {
                    var hitBounds = hitInfo.collider.bounds;
                    var hitPosition = hitInfo.transform.position;
                    Gizmos.color = Color.green;

                    // Check hit left most bound
                    var directionToHit = (hitPosition + (Vector3.forward * hitBounds.extents.z) +
                                          (Vector3.left * hitBounds.extents.x));
                    Gizmos.DrawLine(sensor.transform.position, directionToHit);

                    // Check hit right most bound
                    directionToHit = (hitPosition + (Vector3.forward * hitBounds.extents.z) +
                                      (Vector3.right * hitBounds.extents.x));
                    Gizmos.DrawLine(sensor.transform.position, directionToHit);

                    // Check hit front most bound
                    directionToHit = (hitPosition + (Vector3.back * hitBounds.extents.z) +
                                      (Vector3.left * hitBounds.extents.x));
                    Gizmos.DrawLine(sensor.transform.position, directionToHit);

                    // Check hit back most bound
                    directionToHit = (hitPosition + (Vector3.back * hitBounds.extents.z) +
                                      (Vector3.right * hitBounds.extents.x));
                    Gizmos.DrawLine(sensor.transform.position, directionToHit);
                }
            }*/
        }
        
        private static Mesh CreateFovGizmoMesh(Transform transform, int layerMask, float fieldOfView, float radius)
        {
            const int quality = 5;
            const float edgeDstThreshold = 0.1f;
            const float maskCutawayDst = 0f;

            var mesh = new Mesh();
            int stepCount = Mathf.RoundToInt(fieldOfView * quality);
            float stepAngleSize = fieldOfView / stepCount;
            var viewPoints = new List<Vector3>();
            var oldViewCast = new ViewCastInfo();
            
            for (int i = 0; i <= stepCount; i++)
            {
                float angle = transform.eulerAngles.y - fieldOfView / 2 + stepAngleSize * i;
                ViewCastInfo newViewCast = ViewCast(transform, layerMask, angle, radius);

                if (i > 0)
                {
                    bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                    if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                    {
                        EdgeInfo edge = FindEdge(transform, layerMask, oldViewCast, newViewCast,  radius, edgeDstThreshold, 8);
                        if (edge.pointA != Vector3.zero) viewPoints.Add(edge.pointA);
                        if (edge.pointB != Vector3.zero) viewPoints.Add(edge.pointB);
                    }
                }

                viewPoints.Add(newViewCast.point);
                oldViewCast = newViewCast;
            }

            int vertexCount = viewPoints.Count + 1;
            int[] triangles = new int[(vertexCount - 2) * 3];
            var vertices = new Vector3[vertexCount];
            vertices[0] = Vector3.zero;

            for (int i = 0; i < vertexCount - 1; i++)
            {
                vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.forward * maskCutawayDst;

                if (i < vertexCount - 2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private static EdgeInfo FindEdge(Transform transform, int layerMask, ViewCastInfo minViewCast, ViewCastInfo maxViewCast, float range, float edgeDstThreshold, int edgeResolveIterations)
        {
            float minAngle = minViewCast.angle;
            float maxAngle = maxViewCast.angle;
            Vector3 minPoint = Vector3.zero;
            Vector3 maxPoint = Vector3.zero;

            for (int i = 0; i < edgeResolveIterations; i++)
            {
                float angle = (minAngle + maxAngle) / 2;
                ViewCastInfo newViewCast = ViewCast(transform, layerMask, angle, range);

                bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
                {
                    minAngle = angle;
                    minPoint = newViewCast.point;
                }
                else
                {
                    maxAngle = angle;
                    maxPoint = newViewCast.point;
                }
            }

            return new EdgeInfo(minPoint, maxPoint);
        }

        private static ViewCastInfo ViewCast(Transform transform, int layerMask, float globalAngle, float range)
        {
            Vector3 dir = DirFromAngle(transform, globalAngle, true);
            RaycastHit hit;

            return Physics.Raycast(transform.position, dir, out hit, range, layerMask) ? 
                new ViewCastInfo(true, hit.point, hit.distance, globalAngle) : 
                new ViewCastInfo(false, transform.position + dir * range, range, globalAngle);
        }

        private static Vector3 DirFromAngle(Transform transform, float angleInDegrees, bool angleIsGlobal)
        {
            if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

        private struct ViewCastInfo
        {
            public bool hit;
            public Vector3 point;
            public float dst;
            public float angle;

            public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
            {
                hit = _hit;
                point = _point;
                dst = _dst;
                angle = _angle;
            }
        }

        private struct EdgeInfo
        {
            public Vector3 pointA;
            public Vector3 pointB;

            public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
            {
                pointA = _pointA;
                pointB = _pointB;
            }
        }
    }
}
