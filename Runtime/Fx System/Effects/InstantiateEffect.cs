using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class InstantiateEffect : Effect
    {
        [SerializeField]
        private GameObject? gameObject;

        private Transform? _transformToSpawnAt;
        private readonly List<GameObject> _previewSpawnedObjects = new();

        public override float Duration => 0;

        public override void Initialize(GameObject parentGo)
        {
            _transformToSpawnAt = parentGo.transform;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            if (!gameObject || !_transformToSpawnAt)
            {
                Debug.LogWarning($"{nameof(InstantiateEffect)} requires a {nameof(GameObject)}");
                return;
            }

            GameObject spawned = Object.Instantiate(gameObject, _transformToSpawnAt.position, Quaternion.identity);
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
