using Konfus.Grids;

namespace Konfus.Pathfinding
{
    // TODO: Make an interface so this isn't tied to grid system...
    public class AStarGrid : GridBase
    {
        public override void Generate()
        {
            Generate(pos => new PathNode(this, pos));
        }

        public PathNode? GetPathNode(int x, int y, int z)
        {
            var node = (PathNode?)GetNode(x, y, z);
            return node;
        }

        public void ResetPathNodes()
        {
            foreach (INode? node in Nodes)
            {
                var pathNode = (PathNode)node;
                pathNode.Reset();
            }
        }
    }
}