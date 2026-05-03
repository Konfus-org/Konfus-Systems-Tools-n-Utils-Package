using UnityEngine;
using Konfus.Sensor_Toolkit;
using System.Linq;
using UnityEngine.InputSystem;
using SensorHit = Konfus.Sensor_Toolkit.Sensor.Hit;

namespace Konfus.Input
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField]
        private ScanSensor? interactionSensor;

        public void Interact()
        {
            if (interactionSensor == null || !interactionSensor.Scan())
            {
                return;
            }

            var hits = interactionSensor.Hits ?? Enumerable.Empty<SensorHit>();
            var interactables = hits
                .Select(hit => hit.GameObject.GetComponentInParent<Interactable>())
                .OfType<Interactable>()
                .Distinct();

            foreach (var inter in interactables)
            {
                inter.Interact(this);
            }
        }

        public void HandleInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.canceled)
            {
                Interact();
            }
        }
    }
}
