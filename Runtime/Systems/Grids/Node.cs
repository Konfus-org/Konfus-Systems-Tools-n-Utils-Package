using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Systems.Grids
{
    public class Node : INode
    {
        private readonly IGrid grid;
        private readonly Vector3Int _gridPosition;

        public Node(IGrid owningGrid, Vector3Int gridPositionOnGrid)
        {
            grid = owningGrid;
            _gridPosition = gridPositionOnGrid;
        }

        public Vector3Int GridPosition => _gridPosition;
        public virtual Vector3 WorldPosition => grid.WorldPosFromGridPos(_gridPosition.x, _gridPosition.y, _gridPosition.z);
        
        public INode[] Neighbors
        {
            get
            {
                if (_neighbors == null) CalculateNeighbors();
                return _neighbors;
            }
            set => _neighbors = value;
        }
        private INode[] _neighbors;

        public virtual void CalculateNeighbors()
        {
            var neighbors = new List<INode>();
                
            Vector3Int[] potentialNeighborPositions =
            {
                GridPosition + new Vector3Int(0, 1, 0), 
                GridPosition + new Vector3Int(0, -1, 0),
                GridPosition + new Vector3Int(1, 0, 0), 
                GridPosition + new Vector3Int(-1, 0, 0),
                GridPosition + new Vector3Int(0, 0, 1), 
                GridPosition + new Vector3Int(0, 0, -1),
            };
                
            foreach (var potentialNeighborPosition in potentialNeighborPositions)
            {
                bool isPosInGridBounds = grid.InGridBounds(
                    potentialNeighborPosition.x,
                    potentialNeighborPosition.y,
                    potentialNeighborPosition.z);
                if (!isPosInGridBounds) continue;
                    
                var neighbor = grid.GetNode(potentialNeighborPosition.x, potentialNeighborPosition.y, potentialNeighborPosition.z);
                if (neighbor != null) neighbors.Add(neighbor);
            }
                
            Neighbors = neighbors.ToArray();
        }
    }
}