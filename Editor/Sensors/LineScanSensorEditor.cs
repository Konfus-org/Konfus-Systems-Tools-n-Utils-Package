using System.Linq;
using Konfus.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Sensors
{
    [CustomEditor(typeof(LineScanSensor))]
    internal class LineScanSensorEditor : SensorEditor
    {
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void DrawGizmos(LineScanSensor sensor, GizmoType gizmoType)
        {
            sensor.Scan();
            DrawSensor(sensor);
        }

        private static void DrawSensor(LineScanSensor sensor)
        {
            Gizmos.color = SensorColors.NoHitColor;
            if (sensor.IsTriggered) Gizmos.color = SensorColors.HitColor;

            // transform the gizmo
            Gizmos.matrix *= Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);

            float length = sensor.SensorLength;

            if (sensor is { IsTriggered: true, Hits: not null })
                length = Vector3.Distance(sensor.transform.position, sensor.Hits.First().Point);

            Gizmos.DrawLine(Vector3.zero, Vector3.forward * length);

            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.02f, 0.02f, 0.02f));
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.forward * length, new Vector3(0.02f, 0.02f, 0.02f));
        }
    }
}