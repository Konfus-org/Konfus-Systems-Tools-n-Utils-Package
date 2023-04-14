using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Systems.Grid
{
    public class MonoNode : MonoBehaviour, INode
    {
        private readonly Node _node;
        public MonoNode(GridBase owningGrid, Vector3Int gridPositionOnGrid)
        {
            _node = new Node(owningGrid, gridPositionOnGrid);
        }

        public Vector3Int GetGridPosition() => _node.GetGridPosition();
        public Vector3 GetWorldPosition() => _node.GetWorldPosition();

        public void CalculateNeighbors(INode.NumberConnections numberConnections) =>
            _node.CalculateNeighbors(numberConnections);
       
        public IList<TNode> GetNeighbors<TNode>() where TNode : INode => 
            _node.GetNeighbors<TNode>();
        public IList<INode> GetNeighbors() => _node.GetNeighbors();

        public void AddNeighbor(INode n) => _node.AddNeighbor(n);
        public void AddNeighbors(List<INode> nList) => _node.AddNeighbors(nList);

        public void SetNeighbors(List<INode> nList) => _node.SetNeighbors(nList);
    }
}