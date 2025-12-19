using Konfus.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Tools.Sensors
{
    [CustomEditor(typeof(ArcScanSensor))]
    public class ArcScanSensorEditor : SensorEditor
    {
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void DrawGizmos(ArcScanSensor sensor, GizmoType gizmoType)
        {
            sensor.Scan();
            DrawSensor(sensor);
        }
        
        private static void DrawSensor(ArcScanSensor sensor)
        {
            Gizmos.color = SensorColors.NoHitColor;
            if (sensor.isTriggered) Gizmos.color = SensorColors.HitColor;

            // transform the gizmo
            Gizmos.matrix *= Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);

            float length = sensor.SensorLength;

            Gizmos.matrix = Matrix4x4.identity;

            float step = sensor.ArcAngle / sensor.Resolution;

            Vector3 origin = sensor.transform.position + sensor.transform.forward * sensor.SensorLength;

            // draw an arc
            for (int i = 0; i < sensor.Resolution; i++)
            {
                float prevAngle = step * i;
                float nextAngle = step * (i + 1);

                Vector3 x = -sensor.transform.forward;
                Vector3 y = sensor.transform.up;

                Vector3 prevDir = Mathf.Cos(prevAngle) * x + Mathf.Sin(prevAngle) * y;
                Vector3 nextDir = Mathf.Cos(nextAngle) * x + Mathf.Sin(nextAngle) * y;

                prevDir *= sensor.SensorLength;
                nextDir *= sensor.SensorLength;

                prevDir += origin;
                nextDir += origin;

                // if something was hit something, stop!
                if (Physics.Linecast(prevDir, nextDir, out RaycastHit hit, sensor.DetectionFilter))
                {
                    Gizmos.DrawLine(prevDir, hit.point);

                    //green box
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.1f);

                    Gizmos.DrawWireCube(Vector3.forward * length, new Vector3(0.02f, 0.02f, 0.02f));

                    break;
                }

                Gizmos.DrawLine(prevDir, nextDir);

                if (i == sensor.Resolution - 1)
                {
                    //green box
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(Vector3.forward * length, new Vector3(0.02f, 0.02f, 0.02f));
                }
            }
        }
    }
}
