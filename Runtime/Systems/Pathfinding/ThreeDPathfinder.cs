using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Systems.Grid;
using MartianChild.Utility.Grid_System;
using UnityEngine;

namespace Konfus.Systems.Pathfinding
{
    public class ThreeDPathfinder
    {
        private readonly int _moveToFaceNeighborCost;
        private readonly int _moveToEdgeNeighborCost;
        private readonly int _moveToCornerNeighborCost;

        private readonly ThreeDAStarGrid threeDaStarGrid;

        public ThreeDPathfinder(ThreeDAStarGrid threeDGrid)
        {
            threeDaStarGrid = threeDGrid;
        }

        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition, int[] traversableTypes)
        {
            threeDaStarGrid.GridPosFromWorldPos(startWorldPosition, out int startX, out int startY, out int startZ);
            threeDaStarGrid.GridPosFromWorldPos(endWorldPosition, out int endX, out int endY, out int endZ);

            List<PathThreeDNode> path = FindPath(startX, startY, startZ, endX, endY, endZ, traversableTypes);

            return path?.Select(pathNode => pathNode.WorldPosition).ToList();
        }

        public List<PathThreeDNode> FindPath(int startX, int startY, int startZ, int endX, int endY, int endZ, int[] traversableTypes)
        {
            PathThreeDNode startThreeDNode = threeDaStarGrid.GetPathNode(startX, startY, startZ);
            PathThreeDNode endThreeDNode = threeDaStarGrid.GetPathNode(endX, endY, endZ);

            // invalid path
            if (startThreeDNode == null || endThreeDNode == null || !traversableTypes.Contains(endThreeDNode.Type)) return new List<PathThreeDNode>();

            var validLinkedNodes = new Dictionary<PathThreeDNode, PathThreeDNode>();
            var openList = new HashSet<PathThreeDNode> {startThreeDNode};
            var closedList = new HashSet<PathThreeDNode>();

            threeDaStarGrid.ResetPathNodes();

            startThreeDNode.DistFromStartNode = 0;
            startThreeDNode.EstDistToDestinationNode = CalculateDistanceCost(startThreeDNode, endThreeDNode);

            while (openList.Count > 0)
            {
                PathThreeDNode currentThreeDNode = GetLowestFCostNode(openList);
                if (currentThreeDNode == endThreeDNode) return CalculatePath(endThreeDNode, validLinkedNodes);

                openList.Remove(currentThreeDNode);
                closedList.Add(currentThreeDNode);

                foreach (INode node in currentThreeDNode.Neighbors)
                {
                    var neighbourNode = (PathThreeDNode)node;
                    if (closedList.Contains(neighbourNode)) continue;
                    if (!traversableTypes.Contains(neighbourNode.Type))
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    int tentativeGCost = currentThreeDNode.DistFromStartNode +
                                         CalculateDistanceCost(currentThreeDNode, neighbourNode);
                    if (tentativeGCost >= neighbourNode.DistFromStartNode) continue;

                    validLinkedNodes[neighbourNode] = currentThreeDNode;
                    neighbourNode.DistFromStartNode = tentativeGCost;
                    neighbourNode.EstDistToDestinationNode = CalculateDistanceCost(neighbourNode, endThreeDNode);

                    openList.Add(neighbourNode);
                }
            }

            // No path found...
            return new List<PathThreeDNode>();
        }

        private List<PathThreeDNode> CalculatePath(PathThreeDNode endThreeDNode, Dictionary<PathThreeDNode, PathThreeDNode> validLinkedNodes)
        {
            List<PathThreeDNode> path = new List<PathThreeDNode> {endThreeDNode};
            PathThreeDNode currentThreeDNode = endThreeDNode;
            while (validLinkedNodes.TryGetValue(currentThreeDNode, out var node))
            {
                path.Add(node);
                currentThreeDNode = node;
            }

            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(PathThreeDNode a, PathThreeDNode b)
        {
            int xDistance = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
            int yDistance = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
            int zDistance = Mathf.Abs(a.GridPosition.z - b.GridPosition.z);
            //int remaining = Mathf.Abs(xDistance - yDistance - zDistance);

            var minimum = Math.Min(Math.Min(xDistance, yDistance), zDistance);
            var maximum = Math.Max(Math.Max(xDistance, yDistance), zDistance);

            var tripleAxis = minimum;
            var doubleAxis = xDistance + yDistance + zDistance - maximum - 2 * minimum;
            var singleAxis = maximum - doubleAxis - tripleAxis;

            var approximation = _moveToFaceNeighborCost * singleAxis + 
                                _moveToEdgeNeighborCost * doubleAxis + 
                                _moveToCornerNeighborCost * tripleAxis;
            return approximation;
        }

        private PathThreeDNode GetLowestFCostNode(IEnumerable<PathThreeDNode> pathNodeList)
        {
            PathThreeDNode lowestFCostThreeDNode = pathNodeList.First();
            foreach (var pathNode in pathNodeList)
            {
                if (pathNode.Cost < lowestFCostThreeDNode.Cost) lowestFCostThreeDNode = pathNode;
            }
            
            return lowestFCostThreeDNode;
        }
    }
}