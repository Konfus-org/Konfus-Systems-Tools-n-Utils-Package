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

            Handles.color = Color.white;
            Handles.Label(
                origin + Vector3.up * 0.5f,
                $"Sprinting: {movement.IsSprinting}\n" +
                $"Grounded: {movement.IsGroundedNow}\n"
            );
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
