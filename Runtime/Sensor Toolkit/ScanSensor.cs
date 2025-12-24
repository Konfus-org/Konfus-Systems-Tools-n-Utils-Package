using UnityEngine;

namespace Konfus.Sensor_Toolkit
{
    public abstract class ScanSensor : Sensor
    {
        [SerializeField]
        [Min(0.01f)]
        private float sensorLength = 1f;

        [SerializeField]
        protected QueryTriggerInteraction interactTriggers;

        public float SensorLength => sensorLength;

        public abstract bool Scan();
    }
}