using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Pathfinding
{
    [ExecuteInEditMode]
    public class PathManager : MonoBehaviour
    {
        public PathEvent? onPathFoundEvent;

        [Header("Dependencies")]
        [SerializeField]
        private AStarGrid? grid;

        private Pathfinder? _pathfinder;

        /*
        [Header("A Star Settings")]
        public int moveStraightCost = 10;
        public int moveDiagonalCost = 14;*/

        private void Start()
        {
            if (!grid)
            {
                Debug.LogWarning($"No grid found for {gameObject.name}");
                return;
            }

            _pathfinder = new Pathfinder(grid);
        }

        public AStarGrid GetAStarGrid()
        {
            return grid == null ? throw new NullReferenceException("No grid found") : grid;
        }

        public List<Vector3> FindPath(GameObject requester, Vector3 startWorldPosition, Vector3 desiredWorldDestination,
            int[] traversableNodeTypes)
        {
            if (_pathfinder == null)
            {
                Debug.LogError($"No pathfinder found for {gameObject.name}");
                return new List<Vector3>();
            }

            List<Vector3> path =
                _pathfinder.FindPath(startWorldPosition, desiredWorldDestination, traversableNodeTypes);
            onPathFoundEvent?.Invoke(requester, path);
            return path;
        }
    }

    /// <summary>
    /// <para> The event fired when the pathmanager finds a path. </para>
    /// <param name="GameObject"> The gameobject that requested a path. </param>
    /// <param name="List
    /// 
    /// 
    /// <Vector3>"> The path that is returned. </param>
    /// </summary>
    [Serializable]
    public class PathEvent : UnityEvent<GameObject, List<Vector3>>
    {
    }
}