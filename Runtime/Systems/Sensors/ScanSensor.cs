using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    public abstract class ScanSensor : Sensor
    {
        [PropertyOrder(2)]
        [Header("Params")]
        public float sensorLength = 1f;
        
        public abstract bool Scan();
    }
}