using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Systems.Pathfinding
{
    [ExecuteInEditMode] public class ThreeDPathManager : MonoBehaviour
    {
        public PathEvent onPathFoundEvent;
        
        [Header("Dependencies")] 
        [SerializeField]
        private ThreeDAStarGrid grid;
        private ThreeDPathfinder pathfinder;

        /*
        [Header("A Star Settings")] 
        public int moveStraightCost = 10;
        public int moveDiagonalCost = 14;*/
        
        
        private void Start()
        {
            pathfinder = new ThreeDPathfinder(grid);
        }

        public ThreeDAStarGrid GetAStarGrid()
        {
            return grid;
        }
        
        public List<Vector3> FindPath(GameObject requester, Vector3 startWorldPosition, Vector3 desiredWorldDestination, int[] traversableNodeTypes)
        {
            List<Vector3> path = pathfinder.FindPath(startWorldPosition, desiredWorldDestination, traversableNodeTypes);
            onPathFoundEvent.Invoke(requester, path);
            return path;
        }
    }
    
    /// <summary>
    /// <para> The event fired when the pathmanager finds a path. </para>
    /// <param name="GameObject"> The gameobject that requested a path. </param>
    /// <param name="List<Vector3>"> The path that is returned. </param>
    /// </summary>
    [Serializable] public class PathEvent : UnityEvent<GameObject, List<Vector3>> { }
}
