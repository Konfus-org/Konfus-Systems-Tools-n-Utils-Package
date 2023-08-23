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

            List<ThreeDPathNode> path = FindPath(startX, startY, startZ, endX, endY, endZ, traversableTypes);

            return path?.Select(pathNode => pathNode.WorldPosition).ToList();
        }

        public List<ThreeDPathNode> FindPath(int startX, int startY, int startZ, int endX, int endY, int endZ, int[] traversableTypes)
        {
            ThreeDPathNode startNode = threeDaStarGrid.GetPathNode(startX, startY, startZ);
            ThreeDPathNode endNode = threeDaStarGrid.GetPathNode(endX, endY, endZ);

            // invalid path
            if (startNode == null || endNode == null || !traversableTypes.Contains(endNode.Type)) return new List<ThreeDPathNode>();

            var validLinkedNodes = new Dictionary<ThreeDPathNode, ThreeDPathNode>();
            var openList = new HashSet<ThreeDPathNode> {startNode};
            var closedList = new HashSet<ThreeDPathNode>();

            threeDaStarGrid.ResetPathNodes();

            startNode.DistFromStartNode = 0;
            startNode.EstDistToDestinationNode = CalculateDistanceCost(startNode, endNode);

            while (openList.Count > 0)
            {
                ThreeDPathNode currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode) return CalculatePath(endNode, validLinkedNodes);

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (INode node in currentNode.Neighbors)
                {
                    var neighbourNode = (ThreeDPathNode)node;
                    if (closedList.Contains(neighbourNode)) continue;
                    if (!traversableTypes.Contains(neighbourNode.Type))
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    int tentativeGCost = currentNode.DistFromStartNode +
                                         CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost >= neighbourNode.DistFromStartNode) continue;

                    validLinkedNodes[neighbourNode] = currentNode;
                    neighbourNode.DistFromStartNode = tentativeGCost;
                    neighbourNode.EstDistToDestinationNode = CalculateDistanceCost(neighbourNode, endNode);

                    openList.Add(neighbourNode);
                }
            }

            // No path found...
            return new List<ThreeDPathNode>();
        }

        private List<ThreeDPathNode> CalculatePath(ThreeDPathNode endNode, Dictionary<ThreeDPathNode, ThreeDPathNode> validLinkedNodes)
        {
            List<ThreeDPathNode> path = new List<ThreeDPathNode> {endNode};
            ThreeDPathNode currentNode = endNode;
            while (validLinkedNodes.TryGetValue(currentNode, out var node))
            {
                path.Add(node);
                currentNode = node;
            }

            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(ThreeDPathNode a, ThreeDPathNode b)
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

        private ThreeDPathNode GetLowestFCostNode(IEnumerable<ThreeDPathNode> pathNodeList)
        {
            ThreeDPathNode lowestFCostNode = pathNodeList.First();
            foreach (var pathNode in pathNodeList)
            {
                if (pathNode.Cost < lowestFCostNode.Cost) lowestFCostNode = pathNode;
            }
            
            return lowestFCostNode;
        }
    }
}