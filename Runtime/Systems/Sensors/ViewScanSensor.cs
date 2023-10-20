using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.Sensor_Toolkit
{
    public class ViewScanSensor : ScanSensor
    {
        [Tooltip("Layers that can obstruct the sensor")] 
        [SerializeField]
        [PropertyOrder(1)]
        public LayerMask obstructionFilter;
        
        [Tooltip("Field of view in degrees")] 
        [SerializeField]
        [PropertyOrder(2)]
        public float fov;
        [Tooltip("The distance where object will always be detected no matter what")]

        public override bool Scan()
        {
            isTriggered = false;
            RaycastHit[] hitsArray = Physics.SphereCastAll(transform.position, sensorLength, transform.forward, 0.001f, detectionFilter);

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
                if (Physics.Raycast(transform.position, hitInfo.transform.position - transform.position, out RaycastHit hit, sensorLength, obstructionFilter))
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