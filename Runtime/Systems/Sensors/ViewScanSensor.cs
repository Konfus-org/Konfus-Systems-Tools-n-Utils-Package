using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Extensions;
using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
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
            RaycastHit[] hitsArray = Physics.SphereCastAll(transform.position, SensorLength, transform.forward, 0.001f, DetectionFilter);

            // We didn't hit anything...
            if (hitsArray.Length <= 0) return false;

            // Remove hits not within sight
            var hitsToRemove = new List<RaycastHit>();
            foreach (RaycastHit hitInfo in hitsArray)
            {
                // Is hit within FOV?
                Vector3 targetDir = hitInfo.transform.position - transform.position;
                Vector3 forward = transform.forward;
                float angle = Vector3.Angle(targetDir, forward);
                
                // If dist to hit is greater than inevitable detection distance then we ignore fov
                if (angle >= fov)
                {
                    // We are not within fov and out of inevitable detection range removing hit...
                    hitsToRemove.Add(hitInfo);
                    continue;
                }
                    
                // Is hit obstructed?
                if (Physics.Raycast(transform.position, hitInfo.transform.position - transform.position, out RaycastHit hit, SensorLength, obstructionFilter))
                {
                    if (hit.collider.gameObject != hitInfo.collider.gameObject)
                    {
                        float distHit = Vector3.Distance(transform.position, hitInfo.transform.position);
                        float distObstruct = Vector3.Distance(transform.position, hit.transform.position);
                        //only remove hit if hit is further away than obstruction in same direction
                        //without this, valid hits are being removed also when obstruction is found after valid hit
                        if (distHit > distObstruct)
                        {
                            // Is obstructed removing hit...
                            hitsToRemove.Add(hitInfo);
                        }
                    }
                }
            }

            // Remove hits that were out of FOV or were obstructed
            List<RaycastHit> hitsList = hitsArray.ToList();
            hitsList.RemoveAll(hit => hitsToRemove.Contains(hit));

            // If no hits after removing what is out of sight
            if (hitsList.IsNullOrEmpty()) return false;

            // Convert hits array to hits and return true
            hits = hitsList.Select(hit => new Hit()
                {point = transform.position, normal = hit.normal, gameObject = hit.collider.gameObject}).ToArray();
            isTriggered = true;
            return true;
        }
    }
}