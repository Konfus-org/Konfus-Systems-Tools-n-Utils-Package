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

        // TODO: Break debug drawing code out into editor scripts!
        protected override void DrawSensor()
        {
            // scan the world
            Scan();

            Gizmos.color = nothingDetectedColor;
            if (isTriggered) Gizmos.color = detectedSomethingColor;

            // transform the gizmo
            Gizmos.matrix *= Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            float length = sensorLength;

            Gizmos.matrix = Matrix4x4.identity;

            float step = arcAngle / resolution;

            Vector3 origin = transform.position + transform.forward * sensorLength;

            // draw an arc
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

                // if something was hit something, stop!
                if (Physics.Linecast(prevDir, nextDir, out RaycastHit hit, detectionFilter))
                {
                    Gizmos.DrawLine(prevDir, hit.point);

                    //green box
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.1f);

                    Gizmos.DrawWireCube(Vector3.forward * length, new Vector3(0.02f, 0.02f, 0.02f));

                    break;
                }

                Gizmos.DrawLine(prevDir, nextDir);

                if (i == resolution - 1)
                {
                    //green box
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(Vector3.forward * length, new Vector3(0.02f, 0.02f, 0.02f));
                }
            }
        }
    }
}