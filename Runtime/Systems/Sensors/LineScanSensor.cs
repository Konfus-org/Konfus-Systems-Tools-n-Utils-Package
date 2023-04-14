using System.Linq;
using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    public class LineScanSensor : ScanSensor
    {
        public override bool Scan()
        {
            isTriggered = false;

            if (Physics.Linecast(transform.position, transform.position + transform.forward * sensorLength, out RaycastHit hit,
                    detectionFilter, QueryTriggerInteraction.Ignore))
            {
                var hitsDetected = new Hit[1];
                hitsDetected[0] = new Hit() { point = hit.point, normal = hit.normal, gameObject = hit.collider.gameObject };
                hits = hitsDetected;
                isTriggered = true;
                return true;
            }
            return false;
        }

        protected override void DrawSensor()
        {
            // scan the world
            Scan();

            Gizmos.color = nothingDetectedColor;
            if (isTriggered) Gizmos.color = detectedSomethingColor;

            // transform the gizmo
            Gizmos.matrix *= Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            float length = sensorLength;

            if (isTriggered)
                length = Vector3.Distance(transform.position, hits.First().point);

            Gizmos.DrawLine(Vector3.zero, Vector3.forward * length);

            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.02f, 0.02f, 0.02f));
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.forward * length, new Vector3(0.02f, 0.02f, 0.02f));
        }
    }
}