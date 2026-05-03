using Konfus.Input;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Input
{
    [CustomEditor(typeof(RigidbodyMovement))]
    [CanEditMultipleObjects]
    internal class RigidbodyMovementEditor : UnityEditor.Editor
    {
        private const float DebugSphereRadius = 0.05f;
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
                SyncTargets();
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy)]
        private static void DrawGizmos(RigidbodyMovement movement, GizmoType gizmoType)
        {
            if (!Application.isPlaying)
                return;

            Vector3 origin = movement.transform.position + Vector3.up * 0.1f;
            Vector3 groundPoint = movement.GroundPoint;
            Vector3 groundNormal = movement.GroundNormal;
            RigidbodyJumping? jumping = movement.GetComponent<RigidbodyJumping>();

            Vector3 horizVel = movement.CurrentHorizontalVelocity;
            Vector3 desiredVel = movement.DesiredHorizontalVelocity;
            Vector3 inputDir = movement.InputDirection;
            Vector3 newHorizVel = movement.HorizontalVelocity;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + horizVel * DebugVelocityScale);
            Gizmos.DrawSphere(origin + horizVel * DebugVelocityScale, DebugSphereRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + desiredVel * DebugVelocityScale);
            Gizmos.DrawSphere(origin + desiredVel * DebugVelocityScale, DebugSphereRadius);

            Gizmos.color = Color.yellow;
            Vector3 dir = inputDir.sqrMagnitude > 0.0001f ? inputDir.normalized : Vector3.zero;
            Gizmos.DrawLine(origin, origin + dir);
            Gizmos.DrawSphere(origin + dir, DebugSphereRadius * 0.8f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + newHorizVel * DebugVelocityScale);
            Gizmos.DrawSphere(origin + newHorizVel * DebugVelocityScale, DebugSphereRadius);

            if (movement.HasGroundContact)
            {
                Gizmos.color = movement.IsWalkableGround ? Color.green : new Color(1f, 0.4f, 0f);
                Gizmos.DrawSphere(groundPoint, DebugSphereRadius * 1.2f);
                Gizmos.DrawLine(groundPoint, groundPoint + groundNormal);
            }

            DrawDebugPanel(movement, jumping);
        }

        private static string BuildDebugLabel(RigidbodyMovement movement, RigidbodyJumping? jumping)
        {
            string jumpDebug = jumping
                ? "<b>Jump</b>\n" +
                  $"Grounded: {jumping.IsGroundedNow}\n" +
                  $"State: {jumping.CurrentState}\n" +
                  $"Jumping: {jumping.IsJumping}\n" +
                  $"Buffered: {jumping.HasBufferedJumpNow}\n" +
                  $"Coyote Left: {Mathf.Max(0f, jumping.CoyoteUntil - Time.unscaledTime):F2}\n" +
                  $"Buffer Left: {Mathf.Max(0f, jumping.JumpBufferedUntil - Time.unscaledTime):F2}\n" +
                  $"Vertical Vel: {jumping.VerticalVelocity:F2}\n" +
                  $"Height 01: {jumping.JumpHeight01:F2}\n" +
                  $"Y Window: {jumping.StartY:F2} -> {jumping.PeakY:F2}\n"
                : "<b>Jump</b>\nMissing\n";

            return
                "<b>Movement</b>\n" +
                $"State: {movement.CurrentState}\n" +
                $"Input: {movement.MoveInput}\n" +
                $"Sprinting: {movement.IsSprinting}\n" +
                $"Ascending Release: {movement.IsAscendingFromGround}\n" +
                $"Raw Vel: {FormatVector(movement.RawLinearVelocity)}\n" +
                $"Applied Vel: {FormatVector(movement.LastAppliedVelocity)}\n" +
                $"Delta: {FormatVector(movement.PositionDelta)}\n\n" +
                "<b>Ground</b>\n" +
                $"Grounded: {movement.IsGroundedNow}\n" +
                $"Contact: {movement.HasGroundContact}\n" +
                $"Walkable: {movement.IsWalkableGround}\n" +
                $"Slope: {movement.GroundSurfaceAngle:F1} / {movement.MaxInclineAngle:F1}\n" +
                $"Normal: {FormatVector(movement.GroundNormal)}\n" +
                $"Point: {FormatVector(movement.GroundPoint)}\n\n" +
                jumpDebug;
        }

        private static string FormatVector(Vector3 vector)
        {
            return $"({vector.x:F2}, {vector.y:F2}, {vector.z:F2})";
        }

        private static void DrawDebugPanel(RigidbodyMovement movement, RigidbodyJumping? jumping)
        {
            Vector3 anchorWorld = movement.transform.position +
                                  (movement.transform.right * 1.1f) +
                                  Vector3.up * 1.35f;
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(anchorWorld);

            string label = BuildDebugLabel(movement, jumping);
            GUIStyle debugLabelStyle = GetDebugLabelStyle();
            float height = debugLabelStyle.CalcHeight(new GUIContent(label), DebugPanelWidth);
            Rect rect = new(guiPoint.x, guiPoint.y, DebugPanelWidth, height);

            Handles.BeginGUI();
            EditorGUI.DrawRect(rect, new Color(0.08f, 0.08f, 0.08f, 0.88f));
            GUI.Label(rect, label, debugLabelStyle);
            Handles.EndGUI();
        }

        private static GUIStyle GetDebugLabelStyle()
        {
            if (_debugLabelStyle != null)
                return _debugLabelStyle;

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
                if (targetObject is RigidbodyMovement movement)
                    SyncTarget(movement);
            }
        }

        private static void SyncTarget(RigidbodyMovement movement)
        {
            SyncRigidbodyDefaults(movement);
            SyncSerializedFields(movement);
        }

        private static void SyncRigidbodyDefaults(RigidbodyMovement movement)
        {
            Rigidbody rigidbody = movement.GetComponent<Rigidbody>();
            if (!rigidbody)
                return;

            bool changed = rigidbody.interpolation != RigidbodyInterpolation.Interpolate ||
                           rigidbody.collisionDetectionMode != CollisionDetectionMode.ContinuousDynamic ||
                           rigidbody.constraints != (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ);

            if (!changed)
                return;

            Undo.RecordObject(rigidbody, "Sync Rigidbody Movement");
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            EditorUtility.SetDirty(rigidbody);
        }

        private static void SyncSerializedFields(RigidbodyMovement movement)
        {
            SerializedObject serializedMovement = new(movement);
            serializedMovement.Update();

            SerializedProperty accelerationCurve = serializedMovement.FindProperty("accelerationCurve");
            SerializedProperty decelerationCurve = serializedMovement.FindProperty("decelerationCurve");
            SerializedProperty movementReference = serializedMovement.FindProperty("movementReference");

            bool changed = false;

            AnimationCurve accel = accelerationCurve.animationCurveValue;
            if (EnsureCurveHasEndpoints(ref accel))
            {
                accelerationCurve.animationCurveValue = accel;
                changed = true;
            }

            AnimationCurve decel = decelerationCurve.animationCurveValue;
            if (EnsureCurveHasEndpoints(ref decel))
            {
                decelerationCurve.animationCurveValue = decel;
                changed = true;
            }

            if (!movementReference.objectReferenceValue)
            {
                movementReference.objectReferenceValue = movement.transform;
                changed = true;
            }

            if (!changed)
                return;

            Undo.RecordObject(movement, "Sync Rigidbody Movement");
            serializedMovement.ApplyModifiedProperties();
            EditorUtility.SetDirty(movement);
        }

        private static bool EnsureCurveHasEndpoints(ref AnimationCurve curve)
        {
            bool changed = false;

            if (curve.length == 0)
            {
                curve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
                return true;
            }

            float firstTime = curve.keys[0].time;
            float lastTime = curve.keys[curve.length - 1].time;

            if (firstTime > 0f)
            {
                curve.AddKey(0f, curve.Evaluate(0f));
                changed = true;
            }

            if (lastTime < 1f)
            {
                curve.AddKey(1f, curve.Evaluate(1f));
                changed = true;
            }

            return changed;
        }
    }
}
