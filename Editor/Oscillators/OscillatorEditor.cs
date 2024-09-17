using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace Konfus.Editor.Oscillators
{
    /// <summary>
    ///     Custom Unity inspector for Oscillator.cs.
    /// </summary>
    [CustomEditor(typeof(Oscillator), true)]
    public class OscillatorEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     Draw the default inspector, with a clamped Vector3 on the forceScale.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var oscillator = (Oscillator) target;
            var x = (int)Mathf.Clamp01(oscillator.ForceScale.x);
            var y = (int)Mathf.Clamp01(oscillator.ForceScale.y);
            var z = (int)Mathf.Clamp01(oscillator.ForceScale.z);
            oscillator.ForceScale = new Vector3(x, y, z);
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawOscillatorVisualizationGizmos(Oscillator oscillator, GizmoType state)
        {
            if (!oscillator.DrawDebugVisualization) return;
            DrawOscillatorDebugView(oscillator);
        }
        
        private static void DrawOscillatorDebugView(Oscillator oscillator)
        {
            Vector3 bob = oscillator.transform.localPosition;
            Vector3 equilibrium = oscillator.LocalEquilibriumPosition;
            if (oscillator.transform.parent != null)
            {
                bob += oscillator.transform.parent.position;
                equilibrium += oscillator.transform.parent.position;
            }

            // Draw (wire) equilibrium position
            Color color = Color.green;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(equilibrium, 0.7f);

            // Draw (solid) bob position
            // Color goes from green (0,1,0,0) to yellow (1,1,0,0) to red (1,0,0,0).
            float upperAmplitude = oscillator.Stiffness * oscillator.Mass / (3f * 100f); // Approximately the upper limit of the amplitude within regular use
            color.r = 2f * Mathf.Clamp(Vector3.Magnitude(bob - equilibrium) * upperAmplitude, 0f, 0.5f);
            color.g = 2f * (1f - Mathf.Clamp(Vector3.Magnitude(bob - equilibrium) * upperAmplitude, 0.5f, 1f));
            Gizmos.color = color;
            Gizmos.DrawSphere(bob, 0.75f);
            Gizmos.DrawLine(bob, equilibrium);
        }
    }
}