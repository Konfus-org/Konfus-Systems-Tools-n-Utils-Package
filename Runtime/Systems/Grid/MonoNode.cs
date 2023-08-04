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
    }
}