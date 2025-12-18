using System;
using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Sensor_Toolkit
{
    [ExecuteInEditMode]
    public abstract class Sensor : MonoBehaviour
    {
        [Header("Filters")]
        [Tooltip("The layers detectable by sensor")]
        [SerializeField]
        private LayerMask detectionFilter;

        [NonSerialized]
        public IEnumerable<Hit> hits;
        [NonSerialized]
        public bool isTriggered;

        public LayerMask DetectionFilter => detectionFilter;

        public struct Hit
        {
            public Vector3 point;
            public Vector3 normal;
            public GameObject gameObject;
        }
    }
}