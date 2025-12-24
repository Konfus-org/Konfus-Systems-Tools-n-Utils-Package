using UnityEngine;

namespace Konfus.Sensor_Toolkit
{
    public class LineScanSensor : ScanSensor
    {
        public override bool Scan()
        {
            IsTriggered = false;

            if (!Physics.Linecast(transform.position, transform.position + transform.forward * SensorLength,
                    out RaycastHit hit,
                    DetectionFilter,
                    interactTriggers)) return false;
            var hitsDetected = new Hit[1];
            hitsDetected[0] = new Hit { Point = hit.point, Normal = hit.normal, GameObject = hit.collider.gameObject };
            Hits = hitsDetected;
            IsTriggered = true;
            return true;
        }
    }
}