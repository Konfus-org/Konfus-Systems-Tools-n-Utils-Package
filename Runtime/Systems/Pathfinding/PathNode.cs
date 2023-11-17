using UnityEngine;

namespace Konfus.Systems.Grids
{
    // TODO: make an interface so this isn't tied to the grid system
    public class PathNode : Node
    {
        public int DistFromStartNode; //g
        public int EstDistToDestinationNode; //h
        public int Cost => DistFromStartNode + EstDistToDestinationNode + TypeTraversalCost; //f
        
        public virtual int Type { get; set; }
        public virtual int TypeTraversalCost => 0;
        
        public PathNode(GridBase grid, Vector3Int gridPosition) : base(grid, gridPosition)
        {
            EstDistToDestinationNode = 0;
            DistFromStartNode = int.MaxValue;
        }
        
        public void Reset()
        {
            EstDistToDestinationNode = 0;
            DistFromStartNode = int.MaxValue;
        }
    }
}