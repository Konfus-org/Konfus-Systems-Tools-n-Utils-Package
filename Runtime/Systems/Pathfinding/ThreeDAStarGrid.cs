using MartianChild.Utility.Grid_System;

namespace Konfus.Systems.Pathfinding
{
    // TODO: Make an interface so this isn't tied to grid system...
    public class ThreeDAStarGrid : Grid.ThreeDGrid
    {
        protected override void Generate()
        {
            Generate(pos => new PathThreeDNode(this, pos));
        }

        /*
        {
            base.Initialize();
            base.Generate((grid, gridPosition) => new PathNode(grid, gridPosition), OnNodeAdded);
            CalculateNodeConnections();
        }*/

        public PathThreeDNode GetPathNode(int x, int y, int z)
        {
            return (PathThreeDNode)GetNode(x, y, z);
        }

        public void ResetPathNodes()
        {
            foreach (var node in Nodes)
            {
                var pathNode = (PathThreeDNode)node;
                pathNode.Reset();
            }
        }

        /*private void CalculateNodeConnections()
        {
            _pathNodes = new PathNode[gridObjs.GetLength(0), gridObjs.GetLength(1)];
            Array.Copy(gridObjs, _pathNodes, gridObjs.Length);

            foreach (PathNode pathNode in _pathNodes)
            {
                pathNode.CalculateNeighbors(pathManager.numberNodeConnections);
            }
        }

        private void OnNodeAdded(GridObject gridObject)
        {
            //Debug.Log(gridObject.GetGridPosition());
            PathNode pathNode = gridObject as PathNode;
            if (pathNode == null) return;

            Vector3 rayOrigin = WorldPosFromGridPos(gridObject.GetGridPosition()) + new Vector3(0, transform.localScale.y, 0);
            bool hit = Raycaster.ShootRay(rayOrigin, transform.forward, rayOrigin.y * 2, out RaycastHit raycastHit);
            
            switch (hit)
            {
                case true when pathManager.traversableLayers.Contains(raycastHit.transform.gameObject.layer):
                    pathNode.UpdateWorldPosition(raycastHit.point);
                    pathNode.type = PathNode.Type.Land;
                    break;
                case true:
                    pathNode.UpdateWorldPosition(raycastHit.point);
                    pathNode.type = PathNode.Type.NonTraversable;
                    break;
                default:
                    pathNode.type = PathNode.Type.Air;
                    break;
            }
        }*/
    }
}