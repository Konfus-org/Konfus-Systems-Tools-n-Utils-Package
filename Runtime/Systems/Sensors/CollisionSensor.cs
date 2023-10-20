using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Systems.Sensor_Toolkit
{
    [RequireComponent(typeof(Collider))]
    public class CollisionSensor : Sensor
    {
        [Header("Events")]
        [PropertyOrder(2)]
        [Tooltip("Triggered when collision is first registered.")]
        public CollisionEvent onCollisionEnter;
        [PropertyOrder(2)]
        [Tooltip("Triggered while colliding.")]
        public CollisionEvent onCollisionStay;
        [PropertyOrder(2)]
        [Tooltip("Triggered when collision ends.")]
        public CollisionEvent onCollisionExit;

        private void Start()
        {
            hits = new List<Hit>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!ShouldCollide(other.gameObject)) return;
            OnCollision(other);
            onCollisionEnter.Invoke(gameObject, other.gameObject);
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (!ShouldCollide(other.gameObject)) return;
            
            OnCollision(other);
            onCollisionStay.Invoke(gameObject, other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!ShouldCollide(other.gameObject)) return;
            OnCollisionExit(other.gameObject);
            onCollisionExit.Invoke(gameObject, other.gameObject);
        }

        private void OnCollisionEnter(UnityEngine.Collision collision)
        {
            if (!ShouldCollide(collision.gameObject)) return;
            OnCollision(collision);
            onCollisionEnter.Invoke(gameObject, collision.gameObject);
        }

        private void OnCollisionStay(UnityEngine.Collision collision)
        {
            if (!ShouldCollide(collision.gameObject)) return;
            OnCollision(collision);
            onCollisionStay.Invoke(gameObject, collision.gameObject);
        }

        private void OnCollisionExit(UnityEngine.Collision collision)
        {
            if (!ShouldCollide(collision.gameObject)) return;
            OnCollisionExit(collision.gameObject);
            onCollisionExit.Invoke(gameObject, collision.gameObject);
        }

        private void OnCollision(Collider other)
        {
            List<Hit> newHits = hits.ToList();
            newHits.Add(new Hit() { point = other.ClosestPoint(transform.position), gameObject = other.gameObject });
            hits = newHits;
            isTriggered = true;
        }
        
        private void OnCollision(UnityEngine.Collision collision)
        {
            List<Hit> newHits = hits.ToList();
            newHits.Add(new Hit() { point = collision.GetContact(0).point, normal = collision.GetContact(0).normal, gameObject = collision.gameObject });
            hits = newHits;
            isTriggered = true;
        }
        
        private void OnCollisionExit(GameObject other)
        {
            List<Hit> newHits = hits.ToList();
            newHits.RemoveAll(hit => hit.gameObject = other);
            hits = newHits;
            isTriggered = hits.Any();
        }

        private bool ShouldCollide(GameObject go)
        {
            return detectionFilter.Contains(go.layer);
        }
    }

    /// <summary>
    /// Events for triggers being triggered, the first gameobject is the gameobject that holds this script,
    /// the second is the gameobject that collided with the one that triggers the event
    /// </summary>
    [System.Serializable]
    public class CollisionEvent : UnityEvent<GameObject, GameObject> { }

}