using Konfus.Utility.Extensions;
using Unity.Cinemachine;
using UnityEngine;

namespace Konfus.PlayerInput
{
    public class Reticle : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        [Tooltip("Speed at which the reticle moves to its target position in meters per second")]
        private float moveSpeed;
        [SerializeField] 
        [Tooltip("Amount of pull on camera")]
        private float targetGroupWeight;
        [SerializeField] 
        [Tooltip("Amount of pull on camera")]
        private float targetGroupRadius;

        [Header("References")]
        [SerializeField]
        private GameObject visuals;
        
        private Vector3 _moveTo;
        private Vector3 _lockOnOffset;
        private Transform _lockOn;

        // Moves reticle to a target position
        public void MoveTo(Vector3 target) => _moveTo = target;

        public void Enable()
        {
            visuals.SetActive(true);
            var targetGroup = transform.parent.GetComponentInChildren<CinemachineTargetGroup>();
            if (targetGroup)
                targetGroup.AddMember(transform, targetGroupWeight, targetGroupRadius);
        }

        public void Disable()
        {
            visuals.SetActive(false);
            var targetGroup = transform.parent.GetComponentInChildren<CinemachineTargetGroup>();
            if (targetGroup)
                targetGroup.RemoveMember(transform);
        }

        private void Update()
        {
            transform.MoveTo(_moveTo, Time.deltaTime * moveSpeed);
        }
    }
}
