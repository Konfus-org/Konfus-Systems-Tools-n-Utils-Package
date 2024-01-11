using System.Linq;
using Konfus.Systems.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Sensors
{
    [CustomEditor(typeof(SphereScanSensor))]
    public class SphereScanSensorEditor : UnityEditor.Editor
    {
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void DrawGizmos(SphereScanSensor sensor, GizmoType gizmoType)
        {
            sensor.Scan();
            DrawSensor(sensor);
        }
        
        private static void DrawSensor(SphereScanSensor sensor)
        {
            Gizmos.color = SensorColors.NoHitColor;
            if (sensor.isTriggered) Gizmos.color = SensorColors.HitColor;

            float length = sensor.SensorLength;
            
            switch (sensor.SensorType)
            {
                case SphereScanSensor.Type.Standard:
                {
                    if (sensor.isTriggered)
                    {
                        Gizmos.DrawLine(sensor.transform.position, sensor.hits.First().point);
                        Gizmos.DrawWireSphere( 
                            sensor.transform.position + sensor.transform.forward + (Vector3.forward * sensor.SensorRadius/2), 
                            sensor.SensorRadius);
                        Gizmos.DrawSphere(sensor.hits.First().point, 0.1f);
                    }
                    else
                    {
                        Gizmos.DrawLine(sensor.transform.position, sensor.transform.position + sensor.transform.forward);
                        Gizmos.DrawWireSphere( 
                            sensor.transform.position + sensor.transform.forward + (Vector3.forward * sensor.SensorRadius/2), 
                            sensor.SensorRadius);
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
                    Gizmos.DrawLine(Vector3.up * sensor.SensorRadius, Vector3.up * sensor.SensorRadius + Vector3.forward * length - (Vector3.forward * sensor.SensorRadius/2));
                    Gizmos.DrawLine(-Vector3.up * sensor.SensorRadius, -Vector3.up * sensor.SensorRadius + Vector3.forward * length - (Vector3.forward * sensor.SensorRadius/2));
                    Gizmos.DrawLine(Vector3.right * sensor.SensorRadius, Vector3.right * sensor.SensorRadius + Vector3.forward * length - (Vector3.forward * sensor.SensorRadius/2));
                    Gizmos.DrawLine(-Vector3.right * sensor.SensorRadius, -Vector3.right * sensor.SensorRadius + Vector3.forward * length - (Vector3.forward * sensor.SensorRadius/2));
                    Gizmos.DrawWireSphere(Vector3.zero, sensor.SensorRadius);
                    Gizmos.DrawWireSphere(Vector3.zero + (Vector3.forward * length) - (Vector3.forward * sensor.SensorRadius/2), sensor.SensorRadius);
                    break;
                }
            }
        }
    }
}
