using System;
using UnityEngine;

namespace Konfus.Grids
{
    public class MonoNode : MonoBehaviour, INode
    {
        [SerializeField]
        private GridBase? grid;

        private INode? _node;

        private void Start()
        {
            if (grid == null)
            {
                Debug.LogError($"No grid found for {gameObject.name}");
                return;
            }

            _node = new Node(grid, grid.GridPosFromWorldPos(WorldPosition));
        }

        public Color DebugColor => Color.blue;
        public Vector3Int GridPosition => _node?.GridPosition ?? Vector3Int.zero;
        public Vector3 WorldPosition => transform.position;
        public INode[] Neighbors => _node?.Neighbors ?? Array.Empty<INode>();
    }
}