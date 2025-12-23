using System;
using System.Linq;
using UnityEngine;

namespace Konfus.Sensor_Toolkit
{
    public class BoxScanSensor : ScanSensor
    {
        public enum Type
        {
            Standard,
            CheckHitOnly, // doesn't give you normals (info about collision), just boolean (nothing hit / something hit)
            Full
        }

        [SerializeField]
        private Vector3 sensorSize = Vector3.one;

        [SerializeField]
        private Type sensorType = Type.Standard;

        internal Type SensorType => sensorType;
        internal Vector3 SensorSize => sensorSize;

        public override bool Scan()
        {
            IsTriggered = false;

            switch (sensorType)
            {
                case Type.Standard:
                {
                    if (Physics.BoxCast(
                            transform.position,
                            sensorSize / 2,
                            transform.forward,
                            out RaycastHit hit,
                            transform.rotation,
                            SensorLength,
                            DetectionFilter,
                            interactTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore))
                    {
                        var hitsDetected = new Hit[1];
                        hitsDetected[0] = new Hit
                            { Point = hit.point, Normal = hit.normal, GameObject = hit.collider.gameObject };
                        Hits = hitsDetected;
                        IsTriggered = true;
                        return true;
                    }

                    break;
                }
                case Type.Full:
                {
                    var hitsArray = new RaycastHit[10];
                    int size = Physics.BoxCastNonAlloc(transform.position + transform.forward * sensorSize.z / 2,
                        sensorSize / 2, transform.forward, hitsArray, transform.rotation, SensorLength, DetectionFilter,
                        interactTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);

                    // sort hits by distance
                    if (size > 0)
                    {
                        var filledHits = new RaycastHit[size];
                        hitsArray.CopyTo(filledHits, 0);

                        Array.Sort(filledHits, (s1, s2) =>
                        {
                            if (s1.distance > s2.distance)
                                return 1;
                            if (s2.distance > s1.distance)
                                return -1;
                            return 0;
                        });

                        Hits = filledHits.Select(hit => new Hit
                            { Point = hit.point, GameObject = hit.collider.gameObject, Normal = hit.normal });
                        IsTriggered = true;
                        return true;
                    }

                    break;
                }
                case Type.CheckHitOnly:
                {
                    if (Physics.CheckBox(
                            transform.position + transform.forward * SensorLength,
                            sensorSize / 2,
                            transform.rotation,
                            DetectionFilter,
                            interactTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore))
                    {
                        IsTriggered = true;
                        return true;
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }
    }
}