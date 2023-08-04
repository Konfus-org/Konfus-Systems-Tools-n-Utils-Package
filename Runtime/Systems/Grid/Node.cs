using System.Collections.Generic;
using Konfus.Utility.Type_Manipulation;
using UnityEngine;

namespace Konfus.Systems.Grid
{
    public class Node : INode
    {
        private readonly GridBase _grid;
        private readonly Vector3Int _gridPosition;
        private List<INode> _neighbors;

        public Node(GridBase owningGrid, Vector3Int gridPositionOnGrid)
        {
            _neighbors = new List<INode>();
            _grid = owningGrid;
            _gridPosition = gridPositionOnGrid;
        }
        
        public Vector3Int GetGridPosition() => _gridPosition;
        public Vector3 GetWorldPosition() => _grid.WorldPosFromGridPos(_gridPosition.x, _gridPosition.y, _gridPosition.z);
        
        public void AddNeighbor(INode n)
        {
            _neighbors.Add(n);
        }

        public IList<TNode> GetNeighbors<TNode>() where TNode : INode
        {
            return _neighbors.CastList<INode, TNode>();
        }

        public IList<INode> GetNeighbors()
        {
            return _neighbors;
        }

        public void CalculateNeighbors(INode.ConnectionType connectionType)
        {
            // No connections
            if (connectionType == INode.ConnectionType.None) return;
            
            Vector3Int[] potentialNeighborPositions =
            {
                // Straight connections
                _gridPosition + new Vector3Int(0, 1, 0), 
                _gridPosition + new Vector3Int(0, -1, 0),
                _gridPosition + new Vector3Int(1, 0, 0), 
                _gridPosition + new Vector3Int(-1, 0, 0),
                _gridPosition + new Vector3Int(0, 0, 1), 
                _gridPosition + new Vector3Int(0, 0, -1),
                // Horizontal connections
                _gridPosition + new Vector3Int(1, -1, 0), 
                _gridPosition + new Vector3Int(-1, 1, 0),
                _gridPosition + new Vector3Int(1, 1, 0),
                _gridPosition + new Vector3Int(-1, -1, 0),
                _gridPosition + new Vector3Int(1, -1, 1), 
                _gridPosition + new Vector3Int(-1, 1, 1),
                _gridPosition + new Vector3Int(1, 1, 1),
                _gridPosition + new Vector3Int(-1, -1, 1),
                _gridPosition + new Vector3Int(1, -1, -1), 
                _gridPosition + new Vector3Int(-1, 1, -1),
                _gridPosition + new Vector3Int(1, 1, -1),
                _gridPosition + new Vector3Int(-1, -1, -1)
            };

            for (int p = 0; p < potentialNeighborPositions.Length; p++)
            {
                if (connectionType != INode.ConnectionType.Horizontal && p >= 6) return;
                
                Vector3Int potentialNeighborPosition = potentialNeighborPositions[p];
                if (_grid.InGridBounds(potentialNeighborPosition.x, potentialNeighborPosition.y, potentialNeighborPosition.z))
                {
                    var neighbor = _grid.GetNode(potentialNeighborPosition.x, potentialNeighborPosition.y, potentialNeighborPosition.z);
                    if (neighbor != null) AddNeighbor(neighbor);
                }
            }
        }
        
        public void AddNeighbors(List<INode> nList)
        {
            foreach (INode n in nList) 
                _neighbors.Add(n);
        }
        
        public void SetNeighbors(List<INode> nList)
        {
            _neighbors = nList;
        }
    }
}