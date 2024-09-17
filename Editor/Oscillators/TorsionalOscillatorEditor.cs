using Konfus.Systems.Oscillators;
using Konfus.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Oscillators
{
    [CustomEditor(typeof(TorsionalOscillator), true)]
    public class TorsionalOscillatorEditor : UnityEditor.Editor
    {
        private Texture2D _oscillatorIcon;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawIcon();
        }
        
        private void Awake()
        {
            _oscillatorIcon = Resources.Load<Texture2D>("OscillatorIcon");
        }

        private void DrawIcon()
        {
            // Set icon
            var oscillator = (TorsionalOscillator)target;
            EditorGUIUtility.SetIconForObject(oscillator, _oscillatorIcon);
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawOscillatorVisualizationGizmos(TorsionalOscillator oscillator, GizmoType state)
        {
            if (!oscillator.DrawDebugVisualization) return;
            DrawOscillatorDebugView(oscillator);
        }
        
        private static void DrawOscillatorDebugView(TorsionalOscillator oscillator)
        {
            Vector3 bob = oscillator.transform.position;
            float angle = oscillator.AngularDisplacementMagnitude;

            // Draw (wire) pivot position
            Gizmos.color = Color.white;
            Vector3 pivotPosition = oscillator.transform.TransformPoint(Vector3.Scale(oscillator.LocalPivotPosition, -oscillator.transform.localScale));
            Gizmos.DrawWireSphere(pivotPosition, 0.7f);
            
            // Draw rotation
            Vector3 pos = oscillator.transform.position;
            Gizmos.color = Color.cyan;
            Handles.DrawLine(pos, pos + oscillator.LocalEquilibriumRotation.normalized, 2);
            
            // Draw a cross at the pivot position;
            Vector3 cross1 = new Vector3(1, 0, 1) * 0.7f;
            Vector3 cross2 = new Vector3(1, 0, -1) * 0.7f;
            Gizmos.DrawLine(pivotPosition - cross1, pivotPosition + cross1);
            Gizmos.DrawLine(pivotPosition - cross2, pivotPosition + cross2);

            // Color goes from green (0,1,0,0) to yellow (1,1,0,0) to red (1,0,0,0).
            Color color = Color.green;
            float upperAmplitude = 90f; // Approximately the upper limit of the angle amplitude within regular use
            color.r = 2f * Mathf.Clamp(angle / upperAmplitude, 0f, 0.5f);
            color.g = 2f * (1f - Mathf.Clamp(angle / upperAmplitude, 0.5f, 1f));
            Gizmos.color = color;

            // Draw line to equilibrium
            Gizmos.DrawLine(pivotPosition, bob);

            // Draw (solid) bob position
            Gizmos.DrawSphere(bob, 0.7f);
        }
    }
}