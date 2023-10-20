using Konfus.Systems.Sensor_Toolkit;
using Konfus.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Sensors
{
    [CustomEditor(typeof(CollisionSensor))]
    public class CollisionSensorEditor : UnityEditor.Editor
    {
        [DrawGizmo(GizmoType.Active)]
        private static void DrawGizmos(CollisionSensor sensor, GizmoType gizmoType)
        {
            DrawSensor(sensor);
        }

        private static void DrawSensor(CollisionSensor sensor)
        {
            Gizmos.color = sensor.isTriggered ? SensorColors.HitColor : SensorColors.NoHitColor;
            Gizmos.matrix = Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation, sensor.transform.lossyScale);
            Collider collider = sensor.GetComponent<Collider>();
            switch (collider)
            {
                case BoxCollider boxCollider:
                {
                    Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                    break;
                }
                case SphereCollider sphereCollider:
                {
                    Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
                    break;
                }
                case CapsuleCollider capsuleCollider:
                {
                    Quaternion rotation = Quaternion.identity;
                    switch (capsuleCollider.direction)
                    {
                        case 0: // rotated on Z axis
                            rotation = Quaternion.AngleAxis(90, Vector3.forward);
                            break;
                        case 1: // rotated on Y axis
                            rotation = Quaternion.AngleAxis(90, Vector3.up);
                            break;
                        case 2: // rotated on X axis
                            rotation = Quaternion.AngleAxis(90, Vector3.right);
                            break;
                    }

                    Handles.matrix = Matrix4x4.TRS(sensor.transform.position, sensor.transform.rotation * rotation, sensor.transform.lossyScale);
                    HandlesExtensions.DrawWireCapsule(capsuleCollider.center, capsuleCollider.radius, capsuleCollider.height);
                    break;
                }
                case MeshCollider meshCollider:
                {
                    Gizmos.DrawWireMesh(meshCollider.sharedMesh);
                    break;
                }
                default:
                {
                    Debug.LogError("The attached type of collider is not supported!");
                    break;
                }
            }
        }
    }
}
