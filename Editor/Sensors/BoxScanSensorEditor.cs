using System.Linq;
using Konfus.Systems.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Sensors
{
    [CustomEditor(typeof(BoxScanSensor))]
    public class BoxScanSensorEditor : UnityEditor.Editor
    {
        [DrawGizmo(GizmoType.Selected)]
        private static void OnDrawGizmos(BoxScanSensor sensor, GizmoType gizmoType)
        {
            sensor.Scan();
            DrawSensor(sensor);
        }
        
        private static void DrawSensor(BoxScanSensor sensor)
        {
            Gizmos.color = SensorColors.NoHitColor;
            if (sensor.isTriggered) Gizmos.color = SensorColors.HitColor;
            
            float length = sensor.sensorLength;

            switch (sensor.sensorType)
            {
                case BoxScanSensor.Type.Standard:
                {
                    Gizmos.matrix = Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);
                    if (sensor.isTriggered) length = Vector3.Distance(sensor.transform.position, sensor.hits.First().point);
                    Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, length));
                    Gizmos.DrawWireCube(new Vector3(0, 0, length), sensor.sensorSize);
                    break;
                }
                case BoxScanSensor.Type.Full:
                {
                    if (sensor.isTriggered)
                    {
                        foreach (Sensor.Hit hit in sensor.hits)
                            Gizmos.DrawSphere(hit.point == default ? hit.gameObject.transform.position : hit.point, 0.2f);
                    }
                    
                    Gizmos.matrix = Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);
                    Gizmos.DrawWireCube(
                        new Vector3(0, 0, length + sensor.sensorSize.z)/2, 
                        new Vector3(sensor.sensorSize.x, sensor.sensorSize.y, sensor.sensorSize.z + length));
                    break;
                }
                case BoxScanSensor.Type.CheckHitOnly:
                {
                    Gizmos.matrix = Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);
                    Gizmos.DrawWireCube(new Vector3(0, 0, length), sensor.sensorSize);
                    break;
                }
            }
        }
    }
}
