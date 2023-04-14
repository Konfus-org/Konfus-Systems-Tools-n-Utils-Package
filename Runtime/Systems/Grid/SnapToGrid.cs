using Konfus.Utility.Extensions;
using UnityEngine;

namespace Konfus.Systems.Grid
{
    [ExecuteInEditMode] public class SnapToGrid : MonoBehaviour
    {
        [SerializeField]
        private GridBase grid;

        public bool useLocalPos = true;
        public bool snapToGrid = true;
        public bool sizeToGrid = true;
        
        // Adjust size and gridPosition
        private void Update()
        {
            Snap();
        }

        public void Snap()
        {
            if (snapToGrid)
            {
                if (useLocalPos)
                {
                    Vector3 position = transform.localPosition;
                    position.Snap(grid.CellSize);
                    transform.localPosition = position;
                }
                else
                {
                    Vector3 position = transform.position;
                    position.Snap(grid.CellSize);
                    transform.position = position;
                }
                
            }

            if (sizeToGrid)
            {
                Vector3 localScale = transform.localScale;
                localScale.Snap(grid.CellSize);
                transform.localScale = localScale;
            }
        }
    }  
}