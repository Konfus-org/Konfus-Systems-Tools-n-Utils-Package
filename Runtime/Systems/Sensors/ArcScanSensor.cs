using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    public class ArcScanSensor : ScanSensor
    {
        [PropertyOrder(2)]
        [Range(0, Mathf.PI * 2f)]
        public float arcAngle = Mathf.PI * (3f / 2f);
        [PropertyOrder(2)]
        public int resolution = 5;

        public override bool Scan()
        {
            isTriggered = false;
            float step = arcAngle / resolution;
            Vector3 origin = transform.position + transform.forward * sensorLength;
        
            //calculate arc, cast around it
            for (int i = 0; i < resolution; i++)
            {
                float prevAngle = step * i;
                float nextAngle = step * (i + 1);

                Vector3 x = -transform.forward;
                Vector3 y = transform.up;

                Vector3 prevDir = Mathf.Cos(prevAngle) * x + Mathf.Sin(prevAngle) * y;
                Vector3 nextDir = Mathf.Cos(nextAngle) * x + Mathf.Sin(nextAngle) * y;

                prevDir *= sensorLength;
                nextDir *= sensorLength;

                prevDir += origin;
                nextDir += origin;

                // hit something, stop!
                if (Physics.Linecast(prevDir, nextDir, out RaycastHit hit, detectionFilter, QueryTriggerInteraction.Ignore))
                {
                    var hitsDetected = new Hit[1];
                    hitsDetected[0] = new Hit() { point = hit.point, normal = hit.normal, gameObject = hit.collider.gameObject };
                    hits = hitsDetected;
                    isTriggered = true;
                    return true;
                }
            }
            
            return false;
        }
    }
}