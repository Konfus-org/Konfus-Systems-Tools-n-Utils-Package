using System;
using System.Linq;
using Konfus.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Sensor_Toolkit
{
    [CustomEditor(typeof(BoxScanSensor))]
    internal class BoxScanSensorEditor : SensorEditor
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy)]
        private static void DrawGizmos(BoxScanSensor sensor, GizmoType gizmoType)
        {
            sensor.Scan();
            DrawSensor(sensor);
        }

        private static void DrawSensor(BoxScanSensor sensor)
        {
            Gizmos.color = SensorColors.NoHitColor;
            if (sensor.IsTriggered) Gizmos.color = SensorColors.HitColor;

            float length = sensor.SensorLength;

            switch (sensor.SensorType)
            {
                case BoxScanSensor.Type.Standard:
                {
                    Gizmos.matrix = Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);
                    Gizmos.DrawWireCube(new Vector3(0, 0, length),
                        new Vector3(sensor.SensorSize.x, sensor.SensorSize.y, length));
                    if (sensor.IsTriggered)
                    {
                        if (sensor.Hits != null)
                            length = Vector3.Distance(sensor.transform.position, sensor.Hits.First().Point);
                        Gizmos.DrawSphere(Vector3.forward * length, 0.1f);
                    }

                    Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, length));
                    break;
                }
                case BoxScanSensor.Type.Full:
                {
                    if (sensor is { IsTriggered: true, Hits: not null })
                    {
                        foreach (Sensor.Hit hit in sensor.Hits)
                        {
                            Gizmos.DrawSphere(hit.Point == default ? hit.GameObject.transform.position : hit.Point,
                                0.2f);
                        }
                    }

                    Gizmos.matrix = Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);
                    Gizmos.DrawWireCube(
                        new Vector3(0, 0, length + sensor.SensorSize.y) / 2,
                        new Vector3(sensor.SensorSize.x, sensor.SensorSize.y, length));
                    break;
                }
                case BoxScanSensor.Type.CheckHitOnly:
                {
                    Gizmos.matrix = Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);
                    Gizmos.DrawWireCube(new Vector3(0, 0, length),
                        new Vector3(sensor.SensorSize.x, sensor.SensorSize.y, length));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}