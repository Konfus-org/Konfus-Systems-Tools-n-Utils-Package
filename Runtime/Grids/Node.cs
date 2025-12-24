using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Grids
{
    public class Node : INode
    {
        private readonly IGrid? _grid;
        private readonly Vector3Int _gridPosition;
        private INode[]? _neighbors;

        public Node(IGrid owningGrid, Vector3Int gridPositionOnGrid)
        {
            _grid = owningGrid;
            _gridPosition = gridPositionOnGrid;
        }

        public Color DebugColor => Color.blue;
        public Vector3Int GridPosition => _gridPosition;

        public virtual Vector3 WorldPosition =>
            _grid?.WorldPosFromGridPos(_gridPosition.x, _gridPosition.y, _gridPosition.z) ?? Vector3.zero;

        public INode[]? Neighbors
        {
            get
            {
                if (_neighbors == null) CalculateNeighbors();
                return _neighbors;
            }
            set => _neighbors = value;
        }

        public virtual void CalculateNeighbors()
        {
            if (_grid == null)
                return;

            var neighbors = new List<INode>();

            Vector3Int[] potentialNeighborPositions =
            {
                GridPosition + new Vector3Int(0, 1, 0),
                GridPosition + new Vector3Int(0, -1, 0),
                GridPosition + new Vector3Int(1, 0, 0),
                GridPosition + new Vector3Int(-1, 0, 0),
                GridPosition + new Vector3Int(0, 0, 1),
                GridPosition + new Vector3Int(0, 0, -1)
            };

            foreach (Vector3Int potentialNeighborPosition in potentialNeighborPositions)
            {
                bool isPosInGridBounds = _grid.InGridBounds(
                    potentialNeighborPosition.x,
                    potentialNeighborPosition.y,
                    potentialNeighborPosition.z);
                if (!isPosInGridBounds) continue;

                INode? neighbor = _grid.GetNode(potentialNeighborPosition.x, potentialNeighborPosition.y,
                    potentialNeighborPosition.z);
                if (neighbor != null) neighbors.Add(neighbor);
            }

            Neighbors = neighbors.ToArray();
        }
    }
}