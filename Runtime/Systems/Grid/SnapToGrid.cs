using Konfus.Utility.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Konfus.Systems.Grid
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
        private Grid grid;


        private float _transformChangeDelta;
        private Vector3 _lastPosition;
        
        // Adjust size and gridPosition
        private void Update()
        {
            if (grid == null || !transform.hasChanged) return;
            transform.hasChanged = false;
            Snap();
        }

        private void Snap()
        {
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