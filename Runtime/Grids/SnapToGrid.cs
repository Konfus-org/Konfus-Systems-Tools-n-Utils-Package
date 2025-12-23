using Konfus.Utility.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Konfus.Grids
{
    [ExecuteInEditMode]
    public class SnapToGrid : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private bool snapPosToGrid = true;

        [SerializeField]
        private bool sizeScaleToGrid = true;

        [FormerlySerializedAs("threeDGrid")]
        [Header("Dependencies")]
        [SerializeField]
        private GridBase? grid;

        private Vector3 _lastPosition;

        private float _transformChangeDelta;

        // Adjust size and gridPosition
        private void Update()
        {
            if (!grid || !transform.hasChanged) return;
            transform.hasChanged = false;
            Snap();
        }

        private void Snap()
        {
            if (!grid) return;

            if (snapPosToGrid)
            {
                Vector3 position = transform.position;
                position.Snap(grid.CellSize);
                transform.position = position;
            }

            if (sizeScaleToGrid)
            {
                Vector3 localScale = transform.localScale;
                localScale.Snap(grid.CellSize);
                transform.localScale = localScale;
            }
        }
    }
}