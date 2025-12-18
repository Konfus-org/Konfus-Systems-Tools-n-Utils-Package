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
                            SensorLength,
                            DetectionFilter,
                            interactTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore))
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
                        SensorLength,
                        DetectionFilter,
                        interactTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);

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
                            transform.position + transform.forward * SensorLength,
                            sensorSize/2,
                            transform.rotation,
                            DetectionFilter,
                            interactTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore))
                    {
                        isTriggered = true;
                        return true;
                    }

                    break;
                }
            }

            return false;
        }
    }
}