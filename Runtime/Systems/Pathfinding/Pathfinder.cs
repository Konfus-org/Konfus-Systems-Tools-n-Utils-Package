using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Systems.Grid;
using MartianChild.Utility.Grid_System;
using UnityEngine;

namespace Konfus.Systems.Pathfinding
{
    public class Pathfinder
    {
        private readonly int _moveToFaceNeighborCost;
        private readonly int _moveToEdgeNeighborCost;
        private readonly int _moveToCornerNeighborCost;

        private readonly AStarGrid _aStarGrid;

        public Pathfinder(AStarGrid grid)
        {
            _aStarGrid = grid;
        }

        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition, int[] traversableTypes)
        {
            _aStarGrid.GridPosFromWorldPos(startWorldPosition, out int startX, out int startY, out int startZ);
            _aStarGrid.GridPosFromWorldPos(endWorldPosition, out int endX, out int endY, out int endZ);

            List<PathNode> path = FindPath(startX, startY, startZ, endX, endY, endZ, traversableTypes);

            return path?.Select(pathNode => pathNode.WorldPosition).ToList();
        }

        public List<PathNode> FindPath(int startX, int startY, int startZ, int endX, int endY, int endZ, int[] traversableTypes)
        {
            PathNode startNode = _aStarGrid.GetPathNode(startX, startY, startZ);
            PathNode endNode = _aStarGrid.GetPathNode(endX, endY, endZ);

            // invalid path
            if (startNode == null || endNode == null) return null;

            var validLinkedNodes = new Dictionary<PathNode, PathNode>();
            var openList = new HashSet<PathNode> {startNode};
            var closedList = new HashSet<PathNode>();

            _aStarGrid.ResetPathNodes();

            startNode.DistFromStartNode = 0;
            startNode.EstDistToDestinationNode = CalculateDistanceCost(startNode, endNode);

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode) return CalculatePath(endNode, validLinkedNodes);

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (INode node in currentNode.Neighbors)
                {
                    var neighbourNode = (PathNode)node;
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
            return new List<PathNode>();
        }

        private List<PathNode> CalculatePath(PathNode endNode, Dictionary<PathNode, PathNode> validLinkedNodes)
        {
            List<PathNode> path = new List<PathNode> {endNode};
            PathNode currentNode = endNode;
            while (validLinkedNodes.TryGetValue(currentNode, out var node))
            {
                path.Add(node);
                currentNode = node;
            }

            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(PathNode a, PathNode b)
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

        private PathNode GetLowestFCostNode(IEnumerable<PathNode> pathNodeList)
        {
            PathNode lowestFCostNode = pathNodeList.First();
            foreach (var pathNode in pathNodeList)
            {
                if (pathNode.Cost < lowestFCostNode.Cost) lowestFCostNode = pathNode;
            }
            
            return lowestFCostNode;
        }
    }
}