using Konfus.Utility.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Konfus.Systems.Grid
{
    [ExecuteInEditMode] 
    public class SnapToThreeDGrid : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private bool snapPosToGrid = true;
        [SerializeField]
        private bool sizeScaleToGrid = true;
        
        [FormerlySerializedAs("grid")]
        [Header("Dependencies")]
        [SerializeField]
        private ThreeDGrid threeDGrid;


        private float _transformChangeDelta;
        private Vector3 _lastPosition;
        
        // Adjust size and gridPosition
        private void Update()
        {
            if (threeDGrid == null || !transform.hasChanged) return;
            transform.hasChanged = false;
            Snap();
        }

        private void Snap()
        {
            if (snapPosToGrid)
            {
                Vector3 position = transform.position;
                position.Snap(threeDGrid.CellSize);
                transform.position = position;
            }

            if (sizeScaleToGrid)
            {
                Vector3 localScale = transform.localScale;
                localScale.Snap(threeDGrid.CellSize);
                transform.localScale = localScale;
            }
        }
    }  
}