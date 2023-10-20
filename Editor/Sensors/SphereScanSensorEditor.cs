using System.Linq;
using Konfus.Systems.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Sensors
{
    [CustomEditor(typeof(SphereScanSensor))]
    public class SphereScanSensorEditor : UnityEditor.Editor
    {
        [DrawGizmo(GizmoType.Active)]
        private static void OnDrawGizmos(SphereScanSensor sensor, GizmoType gizmoType)
        {
            sensor.Scan();
            DrawSensor(sensor);
        }
        
        private static void DrawSensor(SphereScanSensor sensor)
        {
            Gizmos.color = SensorColors.NoHitColor;
            if (sensor.isTriggered) Gizmos.color = SensorColors.HitColor;

            float length = sensor.sensorLength;
            
            switch (sensor.sensorType)
            {
                case SphereScanSensor.Type.Standard:
                {
                    if (sensor.isTriggered)
                    {
                        Gizmos.DrawLine(sensor.transform.position, sensor.hits.First().point);
                        Gizmos.DrawWireSphere(sensor.hits.First().point, sensor.sensorRadius);
                    }
                    else
                    {
                        Gizmos.DrawLine(sensor.transform.position, sensor.transform.position + sensor.transform.forward);
                    }
                    break;
                }
                case SphereScanSensor.Type.Full:
                {
                    if (sensor.isTriggered)
                    {
                        foreach (Sensor.Hit hit in sensor.hits)
                            Gizmos.DrawSphere(hit.point, 0.2f);
                    }
                    
                    Gizmos.matrix *= Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);
                    Gizmos.DrawLine(Vector3.up * sensor.sensorRadius, Vector3.up * sensor.sensorRadius + Vector3.forward * length);
                    Gizmos.DrawLine(-Vector3.up * sensor.sensorRadius, -Vector3.up * sensor.sensorRadius + Vector3.forward * length);
                    Gizmos.DrawLine(Vector3.right * sensor.sensorRadius, Vector3.right * sensor.sensorRadius + Vector3.forward * length);
                    Gizmos.DrawLine(-Vector3.right * sensor.sensorRadius, -Vector3.right * sensor.sensorRadius + Vector3.forward * length);
                    Gizmos.DrawWireSphere(Vector3.zero, sensor.sensorRadius);
                    Gizmos.DrawWireSphere(Vector3.zero + (Vector3.forward * length), sensor.sensorRadius);
                    break;
                }
            }
        }
    }
}
