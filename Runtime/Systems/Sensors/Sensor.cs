using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    [ExecuteInEditMode]
    public abstract class Sensor : MonoBehaviour
    {
        [Header("Debugging")]
        [PropertyOrder(0)]
        public Color nothingDetectedColor = Color.gray;
        [PropertyOrder(0)]
        public Color detectedSomethingColor = Color.red;
        
        [Header("Filters")]
        [Tooltip("The layers detectable by sensor")]
        [PropertyOrder(1)]
        public LayerMask detectionFilter;

        [NonSerialized]
        public IEnumerable<Hit> hits;
        [NonSerialized]
        public bool isTriggered;

        protected abstract void DrawSensor();

        private void OnDrawGizmos()
        {
            DrawSensor();
        }

        public struct Hit
        {
            public Vector3 point;
            public Vector3 normal;
            public GameObject gameObject;
        }
    }
}