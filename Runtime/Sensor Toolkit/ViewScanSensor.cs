using System.Collections.Generic;
using Konfus.Utility.Extensions;
using UnityEngine;

namespace Konfus.Sensor_Toolkit
{
    public class ViewScanSensor : ScanSensor
    {
        [Tooltip("Layers that can obstruct the sensor")]
        [SerializeField]
        private LayerMask obstructionFilter;

        [Tooltip("Field of view in degrees")]
        [SerializeField]
        [Range(1, 360)]
        private float fov = 180;

        internal LayerMask ObstructionFilter => obstructionFilter;
        internal float Fov => fov;

        public override bool Scan()
        {
            IsTriggered = false;
            Hits = null;

            var hitsList = new List<Hit>();
            var spherecastHits = new RaycastHit[10];
            int size = Physics.SphereCastNonAlloc(transform.position, SensorLength, transform.forward, spherecastHits,
                0.001f,
                DetectionFilter);

            // We didn't hit anything...
            if (size <= 0) return false;

            var filledHits = new RaycastHit[size];
            spherecastHits.CopyTo(filledHits, 0);

            // Remove hits not within sight
            foreach (RaycastHit hitInfo in filledHits)
            {
                Bounds hitBounds = hitInfo.collider.bounds;
                Vector3 hitPosition = hitInfo.transform.position;

                // Check straight to hit
                Vector3 directionToHit = (hitPosition - transform.position).normalized;
                bool hitIsInView = IsHitInView(hitInfo, directionToHit, out RaycastHit hit);

                // Check front left bound corner
                Vector3 frontLeftPoint = hitPosition + Vector3.forward * hitBounds.extents.z +
                                         Vector3.left * hitBounds.extents.x;
                directionToHit = (frontLeftPoint - transform.position).normalized;
                hitIsInView = hitIsInView || IsHitInView(hitInfo, directionToHit, out hit);

                // Check front right bound corner
                Vector3 frontRightPoint = hitPosition + Vector3.forward * hitBounds.extents.z +
                                          Vector3.right * hitBounds.extents.x;
                directionToHit = (frontRightPoint - transform.position).normalized;
                hitIsInView = hitIsInView || IsHitInView(hitInfo, directionToHit, out hit);

                // Check back left bound corner
                Vector3 backLeftPoint = hitPosition + Vector3.back * hitBounds.extents.z +
                                        Vector3.left * hitBounds.extents.x;
                directionToHit = (backLeftPoint - transform.position).normalized;
                hitIsInView = hitIsInView || IsHitInView(hitInfo, directionToHit, out hit);

                // Check back right bound corner
                Vector3 backRightPoint = hitPosition + Vector3.back * hitBounds.extents.z +
                                         Vector3.right * hitBounds.extents.x;
                directionToHit = (backRightPoint - transform.position).normalized;
                hitIsInView = hitIsInView || IsHitInView(hitInfo, directionToHit, out hit);

                if (hitIsInView)
                    hitsList.Add(new Hit
                    {
                        Point = hit.point,
                        Normal = hit.normal,
                        GameObject = hit.collider.gameObject
                    });
            }

            // If no hits after removing what is out of sight
            if (hitsList.IsNullOrEmpty()) return false;

            // Set hits and return true
            Hits = hitsList;
            IsTriggered = true;
            return true;
        }

        private bool IsHitInView(RaycastHit hitInfo, Vector3 direction, out RaycastHit hit)
        {
            hit = new RaycastHit();

            // Within FOV
            if (Vector3.Angle(direction, transform.forward) >= fov / 2) return false;

            // Line of sight cast
            bool hitNotObstructed = Physics.Raycast(
                transform.position,
                direction,
                out hit,
                SensorLength,
                obstructionFilter);

            // Check if thing is obstructed
            if (hitNotObstructed && hit.collider.gameObject != hitInfo.collider.gameObject)
            {
                float distHit = Vector3.Distance(transform.position, hitInfo.transform.position);
                float distObstruct = Vector3.Distance(transform.position, hit.transform.position);
                //only remove hit if hit is further away than obstruction in same direction
                //without this, valid hits are being removed also when obstruction is found after valid hit
                if (distHit > distObstruct)
                    // Is obstructed removing hit...
                    return false;
            }

            return hitNotObstructed;
        }
    }
}