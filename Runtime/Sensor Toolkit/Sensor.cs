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

        public bool IsTriggered { get; protected set; }
        public IEnumerable<Hit>? Hits { get; protected set; }

        public LayerMask DetectionFilter => detectionFilter;

        public struct Hit
        {
            public Vector3 Point;
            public Vector3 Normal;
            public GameObject GameObject;
        }
    }
}