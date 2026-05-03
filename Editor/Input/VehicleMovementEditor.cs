using Konfus.Vehicles;
using Konfus.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Input
{
    [CustomEditor(typeof(VehicleMovement))]
    [CanEditMultipleObjects]
    internal class VehicleMovementEditor : UnityEditor.Editor
    {
        private const float DebugSphereRadius = 0.07f;
        private const float DebugVelocityScale = 0.15f;
        private const float DebugPanelWidth = 250f;
        private static GUIStyle? _debugLabelStyle;

        private void OnEnable()
        {
            SyncTargets();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            bool changed = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                SyncTargets();
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy)]
        private static void DrawGizmos(VehicleMovement movement, GizmoType gizmoType)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Rigidbody? body = movement.Body;
            if (!body)
            {
                return;
            }

            Vector3 origin = body.worldCenterOfMass + Vector3.up * 0.1f;
            Vector3 planarVelocity = movement.PlanarVelocity;
            Vector3 desiredPlanarVelocity = movement.DesiredPlanarVelocity;
            Vector3 groundNormal = movement.GroundNormal;
            Vector3 forward = Vector3.ProjectOnPlane(movement.transform.forward, groundNormal);
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.ProjectOnPlane(movement.transform.forward, Vector3.up);
            }

            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : movement.transform.forward;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + planarVelocity * DebugVelocityScale);
            Gizmos.DrawSphere(origin + planarVelocity * DebugVelocityScale, DebugSphereRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + desiredPlanarVelocity * DebugVelocityScale);
            Gizmos.DrawSphere(origin + desiredPlanarVelocity * DebugVelocityScale, DebugSphereRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + forward);
            Gizmos.DrawSphere(origin + forward, DebugSphereRadius * 0.9f);

            if (movement.IsGrounded)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(origin, origin + groundNormal);
                Gizmos.DrawSphere(origin + groundNormal, DebugSphereRadius * 0.8f);
            }

            DrawDebugPanel(movement);
        }

        private static void DrawDebugPanel(VehicleMovement movement)
        {
            Vector3 anchorWorld = movement.transform.position +
                                  movement.transform.right * 1.25f +
                                  Vector3.up * 1.35f;
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(anchorWorld);

            string label = BuildDebugLabel(movement);
            GUIStyle debugLabelStyle = GetDebugLabelStyle();
            float height = debugLabelStyle.CalcHeight(new GUIContent(label), DebugPanelWidth);
            Rect rect = new(guiPoint.x, guiPoint.y, DebugPanelWidth, height);

            Handles.BeginGUI();
            EditorGUI.DrawRect(rect, new Color(0.08f, 0.08f, 0.08f, 0.88f));
            GUI.Label(rect, label, debugLabelStyle);
            Handles.EndGUI();
        }

        private static string BuildDebugLabel(VehicleMovement movement)
        {
            Rigidbody? body = movement.Body;

            return
                "<b>Vehicle</b>\n" +
                $"Drive Mode: {movement.DriveMode}\n" +
                $"Grounded: {movement.IsGrounded}\n" +
                $"Sprinting: {movement.IsSprinting}\n" +
                $"Input: {movement.MoveInput}\n" +
                $"Smoothed: {movement.SmoothedMoveInput}\n" +
                $"Forward Speed: {movement.CurrentForwardSpeed:F2}\n" +
                $"Target Speed: {movement.TargetSpeed:F2}\n" +
                $"Turn Speed: {movement.CurrentTurnSpeed:F2}\n" +
                $"Yaw Delta: {movement.CurrentYawDelta:F2}\n" +
                $"Planar Vel: {movement.PlanarVelocity}\n" +
                $"Desired Vel: {movement.DesiredPlanarVelocity}\n" +
                $"Ground Normal: {movement.GroundNormal}\n" +
                $"Kinematic Vel: {movement.KinematicVelocity}\n" +
                $"Body Kinematic: {(body ? body.isKinematic : false)}\n";
        }

        private static GUIStyle GetDebugLabelStyle()
        {
            if (_debugLabelStyle != null)
            {
                return _debugLabelStyle;
            }

            _debugLabelStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 11,
                richText = true,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(8, 8, 6, 6)
            };
            _debugLabelStyle.normal.textColor = Color.white;
            return _debugLabelStyle;
        }

        private void SyncTargets()
        {
            foreach (Object targetObject in targets)
            {
                if (targetObject is VehicleMovement movement)
                {
                    SyncTarget(movement);
                }
            }
        }

        private static void SyncTarget(VehicleMovement movement)
        {
            SerializedObject serializedMovement = new(movement);
            serializedMovement.Update();

            SerializedProperty bodyProperty = serializedMovement.FindProperty("body");
            SerializedProperty groundSensorProperty = serializedMovement.FindProperty("groundSensor");

            bool changed = false;

            if (!bodyProperty.objectReferenceValue)
            {
                Rigidbody? body = movement.GetComponent<Rigidbody>();
                if (body)
                {
                    bodyProperty.objectReferenceValue = body;
                    changed = true;
                }
            }

            if (!groundSensorProperty.objectReferenceValue)
            {
                ScanSensor? groundSensor = movement.GetComponentInChildren<ScanSensor>();
                if (groundSensor)
                {
                    groundSensorProperty.objectReferenceValue = groundSensor;
                    changed = true;
                }
            }

            SyncRigidbodyDefaults(movement);

            if (changed)
            {
                Undo.RecordObject(movement, "Sync Vehicle Movement");
                serializedMovement.ApplyModifiedProperties();
                EditorUtility.SetDirty(movement);
            }
        }

        private static void SyncRigidbodyDefaults(VehicleMovement movement)
        {
            Rigidbody? body = movement.GetComponent<Rigidbody>();
            if (!body)
            {
                return;
            }

            bool changed = !body.isKinematic ||
                           body.useGravity ||
                           body.interpolation != RigidbodyInterpolation.Interpolate ||
                           body.collisionDetectionMode != CollisionDetectionMode.ContinuousSpeculative;

            if (!changed)
            {
                return;
            }

            Undo.RecordObject(body, "Sync Vehicle Rigidbody");
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            EditorUtility.SetDirty(body);
        }
    }
}
