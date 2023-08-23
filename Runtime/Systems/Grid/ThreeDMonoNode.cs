using UnityEngine;
using UnityEngine.Serialization;

namespace Konfus.Systems.Grid
{
    public class ThreeDMonoNode : MonoBehaviour, INode
    {
        [SerializeField]
        private ThreeDGrid grid;
        private INode _node;
        
        public Vector3Int GridPosition => _node.GridPosition;
        public Vector3 WorldPosition => transform.position;
        public INode[] Neighbors => _node.Neighbors;
        
        private void Start()
        {
            _node = new ThreeDNode(grid, grid.GridPosFromWorldPos(WorldPosition));
        }
    }
}