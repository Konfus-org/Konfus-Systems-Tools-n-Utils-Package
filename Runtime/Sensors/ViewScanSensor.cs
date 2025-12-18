using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Extensions;
using UnityEngine;

namespace Konfus.Sensor_Toolkit
{
    public class ViewScanSensor : ScanSensor
    {
        [Tooltip("Layers that can obstruct the sensor"), SerializeField]
        private LayerMask obstructionFilter;
        
        [Tooltip("Field of view in degrees"), SerializeField, Range(1, 360)]
        private float fov = 180;

        internal LayerMask ObstructionFilter => obstructionFilter;
        internal float Fov => fov;

        public override bool Scan()
        {
            isTriggered = false;
            hits = null;
            
            var hitsList = new List<Hit>();
            RaycastHit[] spherecastHits = Physics.SphereCastAll(transform.position, SensorLength, transform.forward, 0.001f, DetectionFilter);
            
            // We didn't hit anything...
            if (spherecastHits.Length <= 0) return false;

            // Remove hits not within sight
            foreach (RaycastHit hitInfo in spherecastHits)
            {
                var hitBounds = hitInfo.collider.bounds;
                var hitPosition = hitInfo.transform.position;
                
                // Check straight to hit
                var directionToHit = (hitPosition - transform.position).normalized;
                var hitIsInView = IsHitInView(hitInfo, directionToHit, out var hit);
                
                // Check front left bound corner
                var frontLeftPoint = (hitPosition + (Vector3.forward * hitBounds.extents.z) + (Vector3.left * hitBounds.extents.x));
                directionToHit = (frontLeftPoint - transform.position).normalized;
                hitIsInView = hitIsInView || IsHitInView(hitInfo, directionToHit, out hit);

                // Check front right bound corner
                var frontRightPoint = (hitPosition + (Vector3.forward * hitBounds.extents.z) + (Vector3.right * hitBounds.extents.x));
                directionToHit = (frontRightPoint - transform.position).normalized;
                hitIsInView = hitIsInView || IsHitInView(hitInfo, directionToHit, out hit);

                // Check back left bound corner
                var backLeftPoint = (hitPosition + (Vector3.back * hitBounds.extents.z) + (Vector3.left * hitBounds.extents.x));
                directionToHit = (backLeftPoint - transform.position).normalized;
                hitIsInView = hitIsInView || IsHitInView(hitInfo, directionToHit, out hit);

                // Check back right bound corner
                var backRightPoint = (hitPosition + (Vector3.back * hitBounds.extents.z) + (Vector3.right * hitBounds.extents.x));
                directionToHit = (backRightPoint - transform.position).normalized;
                hitIsInView = hitIsInView || IsHitInView(hitInfo, directionToHit, out hit);
                
                if (hitIsInView)
                {
                    hitsList.Add(new Hit()
                    {
                        point = hit.point,
                        normal = hit.normal,
                        gameObject = hit.collider.gameObject
                    });
                }
            }

            // If no hits after removing what is out of sight
            if (hitsList.IsNullOrEmpty()) return false;

            // Set hits and return true
            hits = hitsList;
            isTriggered = true;
            return true;
        }

        private bool IsHitInView(RaycastHit hitInfo, Vector3 direction, out RaycastHit hit)
        {
            hit = new RaycastHit();
            
            // Within FOV
            if (Vector3.Angle(direction,  transform.forward) >= fov/2) return false;

            // Line of sight cast
            var hitNotObstructed = Physics.Raycast(
                origin: transform.position, 
                direction: direction,
                hitInfo: out hit, 
                maxDistance: SensorLength, 
                layerMask: obstructionFilter);
            
            // Check if thing is obstructed
            if (hitNotObstructed && hit.collider.gameObject != hitInfo.collider.gameObject)
            {
                float distHit = Vector3.Distance(transform.position, hitInfo.transform.position);
                float distObstruct = Vector3.Distance(transform.position, hit.transform.position);
                //only remove hit if hit is further away than obstruction in same direction
                //without this, valid hits are being removed also when obstruction is found after valid hit
                if (distHit > distObstruct)
                {
                    // Is obstructed removing hit...
                    return false;
                }
            }

            return hitNotObstructed;
        }
    }
}