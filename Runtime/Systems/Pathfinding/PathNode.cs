using Konfus.Systems.Grid;
using UnityEngine;

namespace MartianChild.Utility.Grid_System
{
    public class PathNode : Node
    {
        public enum Type
        {
            Solid,
            Water,
            Air,
            Land
        }

        public Type type;
        public int distFromStartNode; //g
        public int estDistToDestinationNode; //h
        public int Cost => distFromStartNode + estDistToDestinationNode; //f
        public PathNode link;
        
        public PathNode(GridBase grid, Vector3Int gridPosition) : base(grid, gridPosition)
        {
            type = Type.Land;
            estDistToDestinationNode = 0;
            distFromStartNode = int.MaxValue;
        }

        public void Reset()
        {
            estDistToDestinationNode = 0;
            distFromStartNode = int.MaxValue;
        }
    }
}