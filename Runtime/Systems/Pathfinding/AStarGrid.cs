using Konfus.Systems.Grids;

namespace Konfus.Systems.Pathfinding
{
    // TODO: Make an interface so this isn't tied to grid system...
    public class AStarGrid : Grid
    {
        public override void Generate()
        {
            Generate(pos => new PathNode(this, pos));
        }

        public PathNode GetPathNode(int x, int y, int z)
        {
            return (PathNode)GetNode(x, y, z);
        }

        public void ResetPathNodes()
        {
            foreach (var node in Nodes)
            {
                var pathNode = (PathNode)node;
                pathNode.Reset();
            }
        }
    }
}