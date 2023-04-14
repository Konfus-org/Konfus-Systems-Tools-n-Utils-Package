using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    public class ViewScanSensor : ScanSensor
    {
        [SerializeField]
        [PropertyOrder(0)] 
        private Color detectionLineColor = Color.green;
        
        [Tooltip("Layers that can obstruct the sensor")] 
        [SerializeField]
        [PropertyOrder(1)]
        private LayerMask obstructionFilter;
        
        [Tooltip("Field of view in degrees")] 
        [SerializeField]
        [PropertyOrder(2)]
        private float fov;
        [Tooltip("The distance where object will always be detected no matter what")]

        public override bool Scan()
        {
            isTriggered = false;
            RaycastHit[] hitsArray = Physics.SphereCastAll(transform.position, sensorLength, transform.forward, 0.001f, detectionFilter);

            // We didn't hit anything...
            if (hitsArray.Length <= 0) return false;

            // Remove hits not within sight
            var hitsToRemove = new List<RaycastHit>();
            foreach (RaycastHit hitInfo in hitsArray)
            {
                // Is hit within FOV?
                Vector3 targetDir = hitInfo.transform.position - transform.position;
                Vector3 forward = transform.forward;
                float angle = Vector3.Angle(targetDir, forward);
                
                // If dist to hit is greater than inevitable detection distance then we ignore fov
                if (angle >= fov)
                {
                    // We are not within fov and out of inevitable detection range removing hit...
                    hitsToRemove.Add(hitInfo);
                    continue;
                }
                    
                // Is hit obstructed?
                if (Physics.Raycast(transform.position, hitInfo.transform.position - transform.position, out RaycastHit hit, sensorLength, obstructionFilter))
                {
                    if (hit.collider.gameObject != hitInfo.collider.gameObject)
                    {
                        // Is obstructed removing hit...
                        hitsToRemove.Add(hitInfo);
                    }
                }
            }

            // Remove hits that were out of FOV or were obstructed
            List<RaycastHit> hitsList = hitsArray.ToList();
            hitsList.RemoveAll(hit => hitsToRemove.Contains(hit));

            // If no hits after removing what is out of sight
            if (hitsList.IsNullOrEmpty()) return false;

            // Convert hits array to hits and return true
            hits = hitsList.Select(hit => new Hit()
                {point = transform.position, normal = hit.normal, gameObject = hit.collider.gameObject}).ToArray();
            isTriggered = true;
            return true;
        }

        protected override void DrawSensor()
        {
            bool hit = Scan();

            // If we have hits draw line to what was detected
            if (hit)
            {
                foreach (Hit hitInfo in hits)
                {
                    Gizmos.color = detectionLineColor;
                    Gizmos.DrawLine(transform.position, hitInfo.gameObject.transform.position);
                }
            }
            
            // Draw FOV mesh and inevitable detection mesh
            Gizmos.color = nothingDetectedColor;
            if (hit) Gizmos.color = detectedSomethingColor;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawMesh(CreateFovGizmoMesh(fov, sensorLength));
        }
        
        private Mesh CreateFovGizmoMesh(float fieldOfView, float radius)
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
                ViewCastInfo newViewCast = ViewCast(angle, radius);

                if (i > 0)
                {
                    bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                    if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                    {
                        EdgeInfo edge = FindEdge(oldViewCast, newViewCast,  radius, edgeDstThreshold, 8);
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

        private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast, float range, float edgeDstThreshold, int edgeResolveIterations)
        {
            float minAngle = minViewCast.angle;
            float maxAngle = maxViewCast.angle;
            Vector3 minPoint = Vector3.zero;
            Vector3 maxPoint = Vector3.zero;

            for (int i = 0; i < edgeResolveIterations; i++)
            {
                float angle = (minAngle + maxAngle) / 2;
                ViewCastInfo newViewCast = ViewCast(angle, range);

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

        private ViewCastInfo ViewCast(float globalAngle, float range)
        {
            Vector3 dir = DirFromAngle(globalAngle, true);
            RaycastHit hit;

            return Physics.Raycast(transform.position, dir, out hit, range, obstructionFilter) ? 
                new ViewCastInfo(true, hit.point, hit.distance, globalAngle) : 
                new ViewCastInfo(false, transform.position + dir * range, range, globalAngle);
        }

        private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
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