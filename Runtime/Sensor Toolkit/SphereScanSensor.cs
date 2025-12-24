using System;
using System.Linq;
using UnityEngine;

namespace Konfus.Sensor_Toolkit
{
    public class SphereScanSensor : ScanSensor
    {
        public enum Type
        {
            Standard,
            Full
        }

        [SerializeField]
        [Min(0)]
        private float sensorRadius = 0.5f;

        [SerializeField]
        private Type sensorType = Type.Standard;

        public float SensorRadius
        {
            get => sensorRadius;
            set => sensorRadius = value;
        }

        internal Type SensorType => sensorType;

        public override bool Scan()
        {
            IsTriggered = false;

            switch (sensorType)
            {
                case Type.Standard:
                {
                    var standardRay = new Ray(
                        transform.position,
                        transform.forward);
                    if (!Physics.SphereCast(standardRay, sensorRadius, out RaycastHit hit, SensorLength,
                            DetectionFilter,
                            interactTriggers))
                        return false;
                    var hitsDetected = new Hit[1];
                    hitsDetected[0] = new Hit
                        { Point = hit.point, Normal = hit.normal, GameObject = hit.collider.gameObject };
                    Hits = hitsDetected;
                    IsTriggered = true;
                    return true;
                }
                case Type.Full:
                {
                    var hitsArray = new RaycastHit[10];
                    var fullRay = new Ray(transform.position, transform.forward);
                    int numHits = Physics.SphereCastNonAlloc(fullRay, sensorRadius, hitsArray, SensorLength,
                        DetectionFilter,
                        interactTriggers);
                    if (numHits <= 0) return false;

                    var filledHits = new RaycastHit[numHits];
                    for (var hitIndex = 0; hitIndex < numHits; hitIndex++)
                    {
                        filledHits[hitIndex] = hitsArray[hitIndex];
                    }

                    Array.Sort(filledHits, (s1, s2) =>
                    {
                        if (s1.distance > s2.distance)
                            return 1;
                        if (s2.distance > s1.distance)
                            return -1;
                        return 0;
                    });

                    Hits = filledHits.Select(hit => new Hit
                    {
                        Point = hit.point,
                        Normal = hit.normal,
                        GameObject = hit.collider.gameObject
                    }).ToArray();

                    IsTriggered = true;
                    return true;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}