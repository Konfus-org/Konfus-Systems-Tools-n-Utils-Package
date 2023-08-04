using UnityEngine;

namespace Konfus.Systems.Grid
{
    public class MonoNode : MonoBehaviour, INode
    {
        [SerializeField]
        private GridBase grid;
        
        private INode _node;
        
        public Vector3Int GridPosition => _node.GridPosition;
        public Vector3 WorldPosition => transform.position;
        public INode[] Neighbors => _node.Neighbors;
        
        private void Start()
        {
            _node = new Node(grid, grid.GridPosFromWorldPos(WorldPosition));
        }
    }
}