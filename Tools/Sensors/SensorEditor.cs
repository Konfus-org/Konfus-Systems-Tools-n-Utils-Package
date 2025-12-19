using Konfus.Sensor_Toolkit;
using UnityEditor;
using UnityEngine;

namespace Konfus.Tools.Sensors
{
    [CustomEditor(typeof(Sensor), editorForChildClasses: true)]
    public class SensorEditor : UnityEditor.Editor
    {
        private Texture2D _sensorIcon;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawIcon();
        }
        
        private void Awake()
        {
            _sensorIcon = Resources.Load<Texture2D>("SensorIcon");
        }

        private void DrawIcon()
        {
            // Set icon
            var sensor = (Sensor)target;
            EditorGUIUtility.SetIconForObject(sensor, _sensorIcon);
        }
    }
}
