using UnityEngine;

namespace Konfus.Sensor_Toolkit
{
    public class ArcScanSensor : ScanSensor
    {
        [SerializeField, Range(0, Mathf.PI * 2f)]
        private float arcAngle = Mathf.PI * (3f / 2f);
        [SerializeField]
        private int resolution = 5;

        internal float ArcAngle => arcAngle;
        internal float Resolution => resolution;

        public override bool Scan()
        {
            isTriggered = false;
            float step = arcAngle / resolution;
            Vector3 origin = transform.position + transform.forward * SensorLength;
        
            //calculate arc, cast around it
            for (int i = 0; i < resolution; i++)
            {
                float prevAngle = step * i;
                float nextAngle = step * (i + 1);

                Vector3 x = -transform.forward;
                Vector3 y = transform.up;

                Vector3 prevDir = Mathf.Cos(prevAngle) * x + Mathf.Sin(prevAngle) * y;
                Vector3 nextDir = Mathf.Cos(nextAngle) * x + Mathf.Sin(nextAngle) * y;

                prevDir *= SensorLength;
                nextDir *= SensorLength;

                prevDir += origin;
                nextDir += origin;

                // hit something, stop!
                if (Physics.Linecast(prevDir, nextDir, out RaycastHit hit, DetectionFilter, interactTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore))
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