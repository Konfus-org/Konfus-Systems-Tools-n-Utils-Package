using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    public class SphereScanSensor : ScanSensor
    {
        public enum Type
        {
            Standard,
            Full
        }
        
        [PropertyOrder(2)]
        public float sensorRadius = 0.5f;
        [PropertyOrder(2)]
        public Type sensorType = Type.Standard;
    
        public override bool Scan()
        {
            isTriggered = false;

            if (sensorType == Type.Standard)
            {
                if (Physics.SphereCast(new Ray(transform.position + Vector3.forward * sensorRadius/2, transform.forward), sensorRadius, out RaycastHit hit, sensorLength,
                        detectionFilter, QueryTriggerInteraction.Ignore))
                {
                    var hitsDetected = new Hit[1];
                    hitsDetected[0] = new Hit() { point = hit.point, normal = hit.normal, gameObject = hit.collider.gameObject };
                    hits = hitsDetected;
                    isTriggered = true;
                    return true;
                }
            }
            else
            {
                RaycastHit[] hitsArray = Physics.SphereCastAll(transform.position + Vector3.forward * sensorRadius/2, sensorRadius, transform.forward, sensorLength,
                    detectionFilter, QueryTriggerInteraction.Ignore);
                if (hitsArray.Length > 0)
                {
                    Array.Sort(hitsArray, (s1, s2) =>
                    {
                        if (s1.distance > s2.distance)
                            return 1;

                        if (s2.distance > s1.distance)
                            return -1;

                        return 0;
                    });

                    hits = hitsArray.Select(hit => new Hit() { point = hit.point, normal = hit.normal, gameObject = hit.collider.gameObject }).ToArray();
                    isTriggered = true;
                    return true;
                }
            }
        
            return false;
        }

        protected override void DrawSensor()
        {
            // scan the world
            Scan();

            Gizmos.color = nothingDetectedColor;
            if (!isTriggered)
            {
            }
            else
                Gizmos.color = detectedSomethingColor;

            float length = sensorLength;
            
            switch (sensorType)
            {
                case Type.Standard:
                {
                    Gizmos.matrix *= Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    if (isTriggered) length = Vector3.Distance(transform.position, hits.First().point);
                    Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, length));
                    Gizmos.DrawWireSphere(Vector3.zero + Vector3.forward * length, sensorRadius);
                    break;
                }
                case Type.Full:
                {
                    if (isTriggered)
                    {
                        foreach (Hit hit in hits)
                            Gizmos.DrawSphere(hit.point == default ? hit.gameObject.transform.position : hit.point, 0.2f);
                    }
                    
                    float halfThin = sensorRadius;
                    Gizmos.matrix *= Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    Gizmos.DrawLine(Vector3.up * halfThin, Vector3.up * halfThin + Vector3.forward * length);
                    Gizmos.DrawLine(-Vector3.up * halfThin, -Vector3.up * halfThin + Vector3.forward * length);
                    Gizmos.DrawLine(Vector3.right * halfThin, Vector3.right * halfThin + Vector3.forward * length);
                    Gizmos.DrawLine(-Vector3.right * halfThin, -Vector3.right * halfThin + Vector3.forward * length);
                    Gizmos.DrawWireSphere(Vector3.forward * sensorRadius/2, sensorRadius);
                    Gizmos.DrawWireSphere(Vector3.zero + Vector3.forward * length, sensorRadius);
                    break;
                }
            }
        }
    }
}