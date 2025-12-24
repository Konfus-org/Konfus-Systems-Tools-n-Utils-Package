using System;
using Konfus.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Sensor_Toolkit
{
    [CustomEditor(typeof(SphereScanSensor))]
    internal class SphereScanSensorEditor : SensorEditor
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
            Handles.color = SensorColors.NoHitColor;
            if (sensor.IsTriggered)
            {
                Handles.color = SensorColors.HitColor;
                Gizmos.color = SensorColors.HitColor;
            }

            float length = sensor.SensorLength;

            switch (sensor.SensorType)
            {
                case SphereScanSensor.Type.Standard:
                {
                    // Calculate the center of the sphere at the impact point
                    Vector3 startPoint = sensor.transform.position + sensor.transform.forward * length +
                                         sensor.transform.forward * sensor.SensorRadius;
                    Vector3 endPoint = sensor.transform.position + sensor.transform.forward * length +
                                       sensor.transform.forward * length +
                                       sensor.transform.forward * sensor.SensorRadius;
                    Handles.DrawWireDisc(startPoint, sensor.transform.forward, sensor.SensorRadius);
                    Handles.DrawWireDisc(endPoint, sensor.transform.forward, sensor.SensorRadius);

                    // Draw a line representing the full path
                    Gizmos.DrawLine(sensor.transform.position, endPoint);
                    break;
                }
                case SphereScanSensor.Type.Full:
                {
                    if (sensor is { IsTriggered: true, Hits: not null })
                    {
                        foreach (Sensor.Hit hit in sensor.Hits)
                        {
                            Gizmos.DrawSphere(hit.Point, 0.2f);
                        }
                    }

                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix *= Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, Vector3.one);
                    Gizmos.DrawLine(Vector3.up * sensor.SensorRadius,
                        Vector3.up * sensor.SensorRadius + Vector3.forward * length -
                        Vector3.forward * sensor.SensorRadius / 2);
                    Gizmos.DrawLine(-Vector3.up * sensor.SensorRadius,
                        -Vector3.up * sensor.SensorRadius + Vector3.forward * length -
                        Vector3.forward * sensor.SensorRadius / 2);
                    Gizmos.DrawLine(Vector3.right * sensor.SensorRadius,
                        Vector3.right * sensor.SensorRadius + Vector3.forward * length -
                        Vector3.forward * sensor.SensorRadius / 2);
                    Gizmos.DrawLine(-Vector3.right * sensor.SensorRadius,
                        -Vector3.right * sensor.SensorRadius + Vector3.forward * length -
                        Vector3.forward * sensor.SensorRadius / 2);
                    Gizmos.DrawWireSphere(Vector3.zero, sensor.SensorRadius);
                    Gizmos.DrawWireSphere(
                        Vector3.zero + Vector3.forward * length - Vector3.forward * sensor.SensorRadius / 2,
                        sensor.SensorRadius);

                    Gizmos.matrix = oldMatrix;

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}