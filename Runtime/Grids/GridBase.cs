using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Extensions;
using UnityEngine;

namespace Konfus.Grids
{
    public abstract class GridBase : MonoBehaviour, IGrid
    {
        [Header("Settings:")]
        [SerializeField]
        private float cellSize = 1f;

        [SerializeField]
        private Vector3Int scale = Vector3Int.one * 10;

        [Header("Debug:")]
        [SerializeField]
        private bool drawGridCells;

        [SerializeField]
        private bool drawNodes;

        [SerializeField]
        private bool drawNodeConnections;

        [SerializeField]
        private bool drawNodeLabels;

        public INode[,,] NodesXYZ { get; private set; } = new INode[0, 0, 0];

        public bool DrawGridCells => drawGridCells;
        public bool DrawNodes => drawNodes;
        public bool DrawNodeConnections => drawNodeConnections;
        public bool DrawNodeLabels => drawNodeLabels;

        public IEnumerable<INode> Nodes => NodesXYZ.Cast<INode>();
        public Vector3Int Scale => scale;
        public float CellSize => cellSize;

        public bool InGridBounds(int x, int y, int z)
        {
            return !(x < 0 || y < 0 || z < 0 || x >= Scale.x || y >= Scale.y || z >= Scale.z);
        }

        public bool InGridBounds(Vector3 worldPosition)
        {
            GridPosFromWorldPos(worldPosition, out int x, out int y, out int z);
            return !InGridBounds(x, y, z);
        }

        public Vector3 WorldPosFromGridPos(int x, int y, int z)
        {
            Vector3 worldPos = new Vector3(x, y, z)
                * cellSize + transform.position + new Vector3(1, 1, 1) * cellSize / 2;
            worldPos.RotateAroundPivot(transform.position, transform.rotation.eulerAngles);
            return worldPos;
        }

        public Vector3 WorldPosFromGridPos(Vector3Int gridPos)
        {
            return WorldPosFromGridPos(gridPos.x, gridPos.y, gridPos.z);
        }

        public void GridPosFromWorldPos(Vector3 worldPosition, out int x, out int y, out int z)
        {
            // Convert to grid position
            x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / CellSize);
            y = Mathf.FloorToInt((worldPosition.y - transform.position.y) / CellSize);
            z = Mathf.FloorToInt((worldPosition.z - transform.position.z) / CellSize);
        }

        public Vector3Int GridPosFromWorldPos(Vector3 worldPosition)
        {
            GridPosFromWorldPos(worldPosition, out int x, out int y, out int z);
            return new Vector3Int(x, y, z);
        }

        public void SetNode(int x, int y, int z, INode value)
        {
            if (!InGridBounds(x, y, z)) return;
            NodesXYZ[x, y, z] = value;
        }

        public void SetNode(Vector3 worldPosition, INode value)
        {
            GridPosFromWorldPos(worldPosition, out int x, out int y, out int z);
            SetNode(x, y, z, value);
        }

        public INode? GetNode(int x, int y, int z)
        {
            return InGridBounds(x, y, z) ? NodesXYZ[x, y, z] : null;
        }

        public INode? GetNode(Vector3Int gridPosition)
        {
            return GetNode(gridPosition.x, gridPosition.y, gridPosition.z);
        }

        public INode? GetNode(Vector3 worldPosition)
        {
            GridPosFromWorldPos(worldPosition, out int x, out int y, out int z);
            return GetNode(x, y, z);
        }

        public void SetNode(Vector3Int gridPosition, INode value)
        {
            SetNode(gridPosition.x, gridPosition.y, gridPosition.z, value);
        }

        public abstract void Generate();

        internal void SetScale(Vector3Int newScale)
        {
            scale = newScale;
        }

        internal void SetCellSize(float newCellSize)
        {
            cellSize = newCellSize;
        }

        protected void Generate(Func<Vector3Int, INode> createNode)
        {
            NodesXYZ = new INode[Scale.x, Scale.y, Scale.z];
            for (var x = 0; x < NodesXYZ.GetLength(0); x++)
            {
                for (var y = 0; y < NodesXYZ.GetLength(1); y++)
                {
                    for (var z = 0; z < NodesXYZ.GetLength(2); z++)
                    {
                        INode gridObject = createNode(new Vector3Int(x, y, z));
                        NodesXYZ[x, y, z] = gridObject;
                    }
                }
            }
        }
    }
}