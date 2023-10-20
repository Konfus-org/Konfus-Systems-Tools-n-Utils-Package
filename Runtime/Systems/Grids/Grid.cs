using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.Grids
{
    public abstract class Grid : MonoBehaviour, IGrid
    {
        internal bool drawGrid = true;
        internal bool drawNodes = false;
        internal bool drawNodeConnections = false;
        internal bool drawGridCellLabels = false;
        
        internal float cellSize = 1f;
        internal Vector3Int scale = Vector3Int.one * 10;
        internal INode[,,] nodes;

        public IEnumerable<INode> Nodes => nodes.Cast<Node>();
        public Vector3Int Scale => scale;
        public float CellSize => cellSize;
        
        public bool InGridBounds(int x, int y, int z)
        {
            // Debug.Log(x + "," + y + "," + z);
            return !(x < 0 || y < 0 || z < 0 || x >= (int)(cellSize * Scale.x) || y >= (int)(cellSize * Scale.y) || z >= (int)(cellSize * Scale.z));
        }
        
        public bool InGridBounds(Vector3 worldPosition)
        {
            GridPosFromWorldPos(worldPosition, out int x, out int y, out int z);
            if (InGridBounds(x, y, z)) return false;
            return true;
        }
        
        public Vector3 WorldPosFromGridPos(int x, int y, int z)
        {
            Vector3 worldPos = new Vector3(x, y, z)
                * cellSize + transform.position + (new Vector3(1,1,1) * cellSize/2);
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
            nodes[x, y, z] = value;
        }
        
        public void SetNode(Vector3Int gridPosition, INode value)
        {
            SetNode(gridPosition.x, gridPosition.y, gridPosition.z, value);
        }

        public void SetNode(Vector3 worldPosition, INode value)
        {
            GridPosFromWorldPos(worldPosition, out int x, out int y, out int z);
            SetNode(x, y, z, value);
        }

        public INode GetNode(int x, int y, int z)
        {
            if (InGridBounds(x, y, z)) return nodes[x, y, z];
            return null;
        }
        
        public INode GetNode(Vector3Int gridPosition)
        {
            return GetNode(gridPosition.x, gridPosition.y, gridPosition.z);
        }

        public INode GetNode(Vector3 worldPosition)
        {
            GridPosFromWorldPos(worldPosition, out int x, out int y, out int z);
            return GetNode(x, y, z);
        }
        
        [PropertyOrder(10), Button(size: ButtonSizes.Large)]
        public abstract void Generate();
        
        protected void Generate(Func<Vector3Int, INode> createNode)
        {
            nodes = new INode[(int)Scale.x, (int)Scale.y, (int)Scale.z];
            for (int x = 0; x < nodes.GetLength(0); x++)
            {
                for (int y = 0; y < nodes.GetLength(1); y++)
                {
                    for (int z = 0; z < nodes.GetLength(2); z++)
                    {
                        INode gridObject = createNode(new Vector3Int(x, y, z));
                        nodes[x, y, z] = gridObject;
                    }
                }
            }
        }
    }
}