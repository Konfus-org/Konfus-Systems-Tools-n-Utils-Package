using Konfus.PlayerInput;
using Konfus.Pickups;
using Konfus.Sensor_Toolkit;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Characters
{
    public abstract class Character : PlayerPossessable
    {
        private Camera _playerCam;
        
        public override Camera GetCamera() => _playerCam;
        public override void SetCamera(Camera cam) => _playerCam = cam;
        public override void Move(Vector2 input) {}
        public override void Jump() {}
        public override bool HandlePickup(Pickup item) => false;
        public override void HandleDropPickup(Pickup item) {}
        public override void Interact() {}
        public override void Aim(Vector3 target) {}
        public override void PrimaryAction() {}
        public override void SecondaryAction() {}
    }
}