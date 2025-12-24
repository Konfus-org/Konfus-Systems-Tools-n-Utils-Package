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
        private Vector2 sensorSize = Vector2.one;

        [SerializeField]
        private Type sensorType = Type.Standard;

        internal Type SensorType => sensorType;
        internal Vector2 SensorSize => sensorSize;

        public override bool Scan()
        {
            IsTriggered = false;

            switch (sensorType)
            {
                case Type.Standard:
                {
                    if (Physics.BoxCast(
                            transform.position,
                            new Vector3(sensorSize.x, sensorSize.y, SensorLength) / 2,
                            transform.forward,
                            out RaycastHit hit,
                            transform.rotation,
                            SensorLength,
                            DetectionFilter,
                            interactTriggers))
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
                    int numHits = Physics.BoxCastNonAlloc(transform.position + transform.forward * sensorSize.y / 2,
                        new Vector3(sensorSize.x, sensorSize.y, SensorLength) / 2, transform.forward, hitsArray,
                        transform.rotation, SensorLength, DetectionFilter,
                        interactTriggers);
                    if (numHits <= 0) break;

                    var filledHits = new RaycastHit[numHits];
                    for (var hitIndex = 0; hitIndex < numHits; hitIndex++)
                    {
                        filledHits[hitIndex] = hitsArray[hitIndex];
                    }

                    // sort hits by distance
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
                case Type.CheckHitOnly:
                {
                    if (Physics.CheckBox(
                            transform.position + transform.forward * SensorLength,
                            new Vector3(sensorSize.x, sensorSize.y, SensorLength) / 2,
                            transform.rotation,
                            DetectionFilter,
                            interactTriggers))
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