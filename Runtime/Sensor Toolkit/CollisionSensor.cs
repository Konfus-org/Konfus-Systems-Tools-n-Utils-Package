using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Sensor_Toolkit
{
    [RequireComponent(typeof(Collider))]
    public class CollisionSensor : Sensor
    {
        [Header("Events")]
        [SerializeField]
        [Tooltip("Triggered when collision is first registered.")]
        public CollisionEvent? onCollisionEnter;

        [SerializeField]
        [Tooltip("Triggered while colliding.")]
        public CollisionEvent? onCollisionStay;

        [SerializeField]
        [Tooltip("Triggered when collision ends.")]
        public CollisionEvent? onCollisionExit;

        private void Start()
        {
            Hits = new List<Hit>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!ShouldCollide(collision.gameObject)) return;
            OnCollision(collision);
            onCollisionEnter?.Invoke(gameObject, collision.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!ShouldCollide(collision.gameObject)) return;
            OnCollisionExit(collision.gameObject);
            onCollisionExit?.Invoke(gameObject, collision.gameObject);
        }

        private void OnCollisionExit(GameObject other)
        {
            List<Hit> newHits = Hits.ToList();
            newHits.RemoveAll(hit => hit.GameObject = other);
            Hits = newHits;
            IsTriggered = Hits.Any();
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!ShouldCollide(collision.gameObject)) return;
            OnCollision(collision);
            onCollisionStay?.Invoke(gameObject, collision.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!ShouldCollide(other.gameObject)) return;
            OnCollision(other);
            onCollisionEnter?.Invoke(gameObject, other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!ShouldCollide(other.gameObject)) return;
            OnCollisionExit(other.gameObject);
            onCollisionExit?.Invoke(gameObject, other.gameObject);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!ShouldCollide(other.gameObject)) return;

            OnCollision(other);
            onCollisionStay?.Invoke(gameObject, other.gameObject);
        }

        private void OnCollision(Collider other)
        {
            List<Hit> newHits = Hits.ToList();
            newHits.Add(new Hit { Point = other.ClosestPoint(transform.position), GameObject = other.gameObject });
            Hits = newHits;
            IsTriggered = true;
        }

        private void OnCollision(Collision collision)
        {
            List<Hit> newHits = Hits.ToList();
            newHits.Add(new Hit
            {
                Point = collision.GetContact(0).point, Normal = collision.GetContact(0).normal,
                GameObject = collision.gameObject
            });
            Hits = newHits;
            IsTriggered = true;
        }

        private bool ShouldCollide(GameObject go)
        {
            LayerMask filter = DetectionFilter;
            return filter.Contains(go.layer);
        }
    }

    /// <summary>
    ///     Events for triggers being triggered, the first gameobject is the gameobject that holds this script,
    ///     the second is the gameobject that collided with the one that triggers the event
    /// </summary>
    [Serializable]
    public class CollisionEvent : UnityEvent<GameObject, GameObject>
    {
    }
}