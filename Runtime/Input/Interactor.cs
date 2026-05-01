using UnityEngine;
using Konfus.Sensor_Toolkit;
using System.Linq;
using System;

namespace Konfus.Input
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField]
        private ScanSensor? interactionSensor;

        public void Interact()
        {
            var scanResult = interactionSensor?.Scan() ?? false;
            if (scanResult)
            {
                var hits = interactionSensor?.Hits ?? Array.Empty<Sensor.Hit>();
                var interactables = hits
                    ?.Select(hit => hit.GameObject.GetComponent<Interactable>())
                    .Where(inter => inter != null) ?? Array.Empty<Interactable>();
                foreach (var inter in interactables)
                {
                    inter.Interact(this);
                }
            }
        }
    }
}