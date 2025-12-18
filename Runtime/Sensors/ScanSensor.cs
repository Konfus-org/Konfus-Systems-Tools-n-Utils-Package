using UnityEngine;

namespace Konfus.Sensor_Toolkit
{
    public abstract class ScanSensor : Sensor
    {
        [SerializeField]
        private float sensorLength = 1f;

        [SerializeField]
        protected bool interactTriggers = false;

        public float SensorLength => sensorLength;
        
        public abstract bool Scan();
    }
}