using UnityEngine;

namespace Konfus.Systems.Grid
{
    public class Node : INode
    {
        private readonly GridBase _grid;
        private readonly Vector3Int _gridPosition;

        public Node(GridBase owningGrid, Vector3Int gridPositionOnGrid)
        {
            _grid = owningGrid;
            _gridPosition = gridPositionOnGrid;
        }
        
        public Vector3Int GetGridPosition() => _gridPosition;
        public Vector3 GetWorldPosition() => _grid.WorldPosFromGridPos(_gridPosition.x, _gridPosition.y, _gridPosition.z);
    }
}