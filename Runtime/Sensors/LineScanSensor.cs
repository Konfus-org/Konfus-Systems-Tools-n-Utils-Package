using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    public class LineScanSensor : ScanSensor
    {
        public override bool Scan()
        {
            isTriggered = false;

            if (Physics.Linecast(transform.position, transform.position + transform.forward * SensorLength, out RaycastHit hit,
                    DetectionFilter, QueryTriggerInteraction.Ignore))
            {
                var hitsDetected = new Hit[1];
                hitsDetected[0] = new Hit() { point = hit.point, normal = hit.normal, gameObject = hit.collider.gameObject };
                hits = hitsDetected;
                isTriggered = true;
                return true;
            }
            return false;
        }
    }
}