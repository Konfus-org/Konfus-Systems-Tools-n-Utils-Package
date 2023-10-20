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

            if (sensorType == Type.Standard && sensorLength != 0)
            {
                var ray = new Ray(transform.position + Vector3.forward * sensorRadius / 2, transform.forward);
                if (Physics.SphereCast(ray, sensorRadius, out RaycastHit hit, sensorLength, detectionFilter, QueryTriggerInteraction.Ignore))
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
                RaycastHit[] hitsArray = Physics.SphereCastAll(
                    transform.position + Vector3.forward * sensorRadius/2, 
                    sensorRadius, 
                    transform.forward, 
                    sensorLength,
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

                    hits = hitsArray.Select(hit => new Hit() 
                    {
                        point = hit.point, 
                        normal = hit.normal, 
                        gameObject = hit.collider.gameObject 
                    }).ToArray();
                    isTriggered = true;
                    return true;
                }
            }
        
            return false;
        }
    }
}