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

        private List<PathNode> _openList;
        private List<PathNode> _closedList;

        public Pathfinder(AStarGrid grid)
        {
            _aStarGrid = grid;
        }

        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition,
            PathNode.Type[] traversableTypes)
        {
            _aStarGrid.GridPosFromWorldPos(startWorldPosition, out int startX, out int startY, out int startZ);
            _aStarGrid.GridPosFromWorldPos(endWorldPosition, out int endX, out int endY, out int endZ);

            List<PathNode> path = FindPath(startX, startY, startZ, endX, endY, endZ, traversableTypes);

            return path?.Select(pathNode => pathNode.WorldPosition).ToList();
        }

        public List<PathNode> FindPath(int startX, int startY, int startZ, int endX, int endY, int endZ, PathNode.Type[] traversableTypes)
        {
            PathNode startNode = _aStarGrid.GetPathNode(startX, startY, startZ);
            PathNode endNode = _aStarGrid.GetPathNode(endX, endY, endZ);

            // invalid path
            if (startNode == null || endNode == null) return null;

            _openList = new List<PathNode> {startNode};
            _closedList = new List<PathNode>();

            _aStarGrid.ResetPathNodes();

            startNode.distFromStartNode = 0;
            startNode.estDistToDestinationNode = CalculateDistanceCost(startNode, endNode);

            while (_openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(_openList);
                if (currentNode == endNode) return CalculatePath(endNode);

                _openList.Remove(currentNode);
                _closedList.Add(currentNode);

                foreach (INode node in currentNode.Neighbors)
                {
                    var neighbourNode = (PathNode)node;
                    if (_closedList.Contains(neighbourNode)) continue;
                    if (!traversableTypes.Contains(neighbourNode.type))
                    {
                        _closedList.Add(neighbourNode);
                        continue;
                    }

                    int tentativeGCost = currentNode.distFromStartNode +
                                         CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost >= neighbourNode.distFromStartNode) continue;

                    neighbourNode.link = currentNode;
                    neighbourNode.distFromStartNode = tentativeGCost;
                    neighbourNode.estDistToDestinationNode = CalculateDistanceCost(neighbourNode, endNode);

                    if (!_openList.Contains(neighbourNode)) _openList.Add(neighbourNode);
                }
            }

            // Out of nodes on the openList
            return null;
        }

        private List<PathNode> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode> {endNode};
            PathNode currentNode = endNode;
            while (currentNode.link != null)
            {
                path.Add(currentNode.link);
                currentNode = currentNode.link;
            }

            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.GridPosition.x - a.GridPosition.x);
            int yDistance = Mathf.Abs(a.GridPosition.y - a.GridPosition.y);
            int zDistance = Mathf.Abs(a.GridPosition.z - a.GridPosition.z);
            //int remaining = Mathf.Abs(xDistance - yDistance - zDistance);

            var minimum = Math.Min(Math.Min(xDistance, yDistance), zDistance);
            var maximum = Math.Max(Math.Max(xDistance, yDistance), zDistance);

            var tripleAxis = minimum;
            var doubleAxis = xDistance + yDistance + zDistance - maximum - 2 * minimum;
            var singleAxis = maximum - doubleAxis - tripleAxis;

            var approximation = _moveToFaceNeighborCost * singleAxis 
                                + _moveToEdgeNeighborCost * doubleAxis 
                                + _moveToCornerNeighborCost * tripleAxis;
            return approximation;
        }

        private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
        {
            PathNode lowestFCostNode = pathNodeList[0];
            for (int i = 1; i < pathNodeList.Count; i++)
            {
                if (pathNodeList[i].Cost < lowestFCostNode.Cost) lowestFCostNode = pathNodeList[i];
            }

            return lowestFCostNode;
        }
    }
}