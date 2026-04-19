using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class InstantiateAndRotateToTarget : Effect
    {
        [SerializeField]
        private Transform? target;

        [SerializeField]
        private GameObject? gameObject;

        [SerializeField]
        private Vector3 rotation;

        private readonly List<GameObject> _previewSpawnedObjects = new();

        public override float Duration => 0;

        public override void Play()
        {
            if (!gameObject || !target)
            {
                Debug.LogWarning($"{nameof(InstantiateAndRotateToTarget)} requires a game object");
                return;
            }

            GameObject spawned =
                Object.Instantiate(gameObject, target.position, Quaternion.Euler(target.rotation.eulerAngles + rotation));

            _previewSpawnedObjects.Add(spawned);
        }

        public override void Pause()
        {
            // do nothing...
        }

        public override void Reset()
        {
            if (_previewSpawnedObjects.Count == 0) return;

            for (int i = _previewSpawnedObjects.Count - 1; i >= 0; i--)
            {
                GameObject previewSpawnedObject = _previewSpawnedObjects[i];
                if (previewSpawnedObject != null)
                {
                    if (Application.isPlaying)
                        Object.Destroy(previewSpawnedObject);
                    else
                        Object.DestroyImmediate(previewSpawnedObject);
                }
            }

            _previewSpawnedObjects.Clear();
        }
    }
}
