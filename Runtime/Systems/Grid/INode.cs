using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Systems.Grid
{
    public interface INode
    {
        public enum ConnectionType
        {
            None, Straight, Horizontal
        }
        
        Vector3Int GetGridPosition();
        Vector3 GetWorldPosition();
        void CalculateNeighbors(ConnectionType connectionType);
        void AddNeighbor(INode n);
        void AddNeighbors(List<INode> nList);
        IList<TNode> GetNeighbors<TNode>() where TNode : INode;
        IList<INode> GetNeighbors();
        void SetNeighbors(List<INode> nList);
    }
}