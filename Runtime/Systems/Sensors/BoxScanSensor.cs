using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    public class BoxScanSensor : ScanSensor
    {
        public enum Type
        {
            Standard,
            CheckHitOnly, // doesn't give you normals (info about collision), just boolean (nothing hit / something hit)
            Full
        }
    
        [PropertyOrder(2)]
        public Vector3 sensorSize = Vector3.one;
        [PropertyOrder(2)]
        public Type sensorType = Type.Standard;

        public override bool Scan()
        {        
            isTriggered = false;
            switch (sensorType)
            {
                case Type.Standard:
                {
                    if (Physics.BoxCast(
                            transform.position,
                            sensorSize/2,
                            transform.forward,
                            out RaycastHit hit,
                            transform.rotation,
                            sensorLength,
                            detectionFilter,
                            QueryTriggerInteraction.Ignore))
                    {
                        var hitsDetected = new Hit[1];
                        hitsDetected[0] = new Hit()
                            {point = hit.point, normal = hit.normal, gameObject = hit.collider.gameObject};
                        hits = hitsDetected;
                        isTriggered = true;
                        return true;
                    }

                    break;
                }
                case Type.Full:
                {
                    RaycastHit[] hitsArray = Physics.BoxCastAll(
                        transform.position + transform.forward * sensorSize.z/2,
                        sensorSize/2,
                        transform.forward,
                        transform.rotation,
                        sensorLength,
                        detectionFilter,
                        QueryTriggerInteraction.Ignore);

                    // sort hits by distance
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

                        hits = hitsArray.Select(hit => new Hit()
                            {point = hit.point, gameObject = hit.collider.gameObject, normal = hit.normal});
                        isTriggered = true;
                        return true;
                    }

                    break;
                }
                case Type.CheckHitOnly:
                {
                    if (Physics.CheckBox(
                            transform.position + transform.forward * sensorLength,
                            sensorSize/2,
                            transform.rotation,
                            detectionFilter,
                            QueryTriggerInteraction.Ignore))
                    {
                        isTriggered = true;
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        protected override void DrawSensor()
        {
            // scan the world
            Scan();

            Gizmos.color = nothingDetectedColor;
            if (isTriggered) Gizmos.color = detectedSomethingColor;
            
            float length = sensorLength;

            switch (sensorType)
            {
                case Type.Standard:
                {
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    if (isTriggered) length = Vector3.Distance(transform.position, hits.First().point);
                    Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, length));
                    Gizmos.DrawWireCube(new Vector3(0, 0, length), sensorSize);
                    break;
                }
                case Type.Full:
                {
                    if (isTriggered)
                    {
                        foreach (Hit hit in hits)
                            Gizmos.DrawSphere(hit.point == default ? hit.gameObject.transform.position : hit.point, 0.2f);
                    }
                    
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    Gizmos.DrawWireCube(new Vector3(0, 0, length + sensorSize.z)/2, new Vector3(sensorSize.x, sensorSize.y, sensorSize.z + length));
                    break;
                }
                case Type.CheckHitOnly:
                {
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    Gizmos.DrawWireCube(new Vector3(0, 0, length), sensorSize);
                    break;
                }
            }
        }
    }
}