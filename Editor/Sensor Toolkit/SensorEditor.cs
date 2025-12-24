using Konfus.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Sensor_Toolkit
{
    [CustomEditor(typeof(Sensor), true)]
    internal class SensorEditor : UnityEditor.Editor
    {
        private Texture2D? _sensorIcon;

        private void Awake()
        {
            _sensorIcon = Resources.Load<Texture2D>("SensorIcon");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawIcon();
        }

        private void DrawIcon()
        {
            // Set icon
            var sensor = (Sensor)target;
            EditorGUIUtility.SetIconForObject(sensor, _sensorIcon);
        }
    }
}