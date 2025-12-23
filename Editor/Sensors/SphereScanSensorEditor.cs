using System;
using System.Linq;
using Konfus.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Sensors
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
            if (sensor.IsTriggered) Gizmos.color = SensorColors.HitColor;

            float length = sensor.SensorLength;

            switch (sensor.SensorType)
            {
                case SphereScanSensor.Type.Standard:
                {
                    if (sensor is { IsTriggered: true, Hits: not null })
                    {
                        Gizmos.DrawLine(sensor.transform.position, sensor.Hits.First().Point);
                        Gizmos.DrawWireSphere(
                            sensor.transform.position + sensor.transform.forward +
                            Vector3.forward * sensor.SensorRadius / 2,
                            sensor.SensorRadius);
                        Gizmos.DrawSphere(sensor.Hits.First().Point, 0.1f);
                    }
                    else
                    {
                        Gizmos.DrawLine(sensor.transform.position,
                            sensor.transform.position + sensor.transform.forward);
                        Gizmos.DrawWireSphere(
                            sensor.transform.position + sensor.transform.forward +
                            Vector3.forward * sensor.SensorRadius / 2,
                            sensor.SensorRadius);
                    }

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
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}