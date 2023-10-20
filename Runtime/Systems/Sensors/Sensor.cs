using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    [ExecuteInEditMode]
    public abstract class Sensor : MonoBehaviour
    {
        [Header("Filters")]
        [Tooltip("The layers detectable by sensor")]
        [PropertyOrder(1)]
        public LayerMask detectionFilter;

        [NonSerialized]
        public IEnumerable<Hit> hits;
        [NonSerialized]
        public bool isTriggered;

        public struct Hit
        {
            public Vector3 point;
            public Vector3 normal;
            public GameObject gameObject;
        }
    }
}