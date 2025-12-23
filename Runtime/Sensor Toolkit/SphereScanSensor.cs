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

            if (sensorType == Type.Standard && SensorLength != 0)
            {
                var ray = new Ray(transform.position + Vector3.forward * sensorRadius / 2, transform.forward);
                if (!Physics.SphereCast(ray, sensorRadius, out RaycastHit hit, SensorLength, DetectionFilter,
                        interactTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore))
                    return false;
                var hitsDetected = new Hit[1];
                hitsDetected[0] = new Hit
                    { Point = hit.point, Normal = hit.normal, GameObject = hit.collider.gameObject };
                Hits = hitsDetected;
                IsTriggered = true;
                return true;
            }

            var hitsArray = new RaycastHit[10];
            int size = Physics.SphereCastNonAlloc(transform.position + Vector3.forward * sensorRadius / 2, sensorRadius,
                transform.forward, hitsArray, SensorLength, DetectionFilter,
                interactTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);
            if (size <= 0) return false;

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
            {
                Point = hit.point,
                Normal = hit.normal,
                GameObject = hit.collider.gameObject
            }).ToArray();

            IsTriggered = true;
            return true;
        }
    }
}