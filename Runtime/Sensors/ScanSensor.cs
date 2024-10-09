using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    public abstract class ScanSensor : Sensor
    {
        [SerializeField]
        private float sensorLength = 1f;

        public float SensorLength => sensorLength;
        
        public abstract bool Scan();
    }
}