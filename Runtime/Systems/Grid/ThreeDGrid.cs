﻿using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Extensions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEditor;

namespace Konfus.Systems.Grid
{
    [ExecuteInEditMode]
    public abstract class ThreeDGrid : MonoBehaviour, IGrid
    {
        [Header("Settings")]
        [PropertyOrder(2), SerializeField, Tooltip("Can also update this by pressing ctrl and scaling the transform this script is attached to.")] 
        private float cellSize = 1f;
        [PropertyOrder(2), SerializeField, Tooltip("Can also update this by scaling the transform this script is attached to.")] 
        private Vector3Int scale = Vector3Int.one * 10;
        [Header("Debug")]
        [PropertyOrder(3), SerializeField]
        private bool drawGrid = true;
        [PropertyOrder(3), SerializeField]
        private bool drawNodes = false;
        [PropertyOrder(3), SerializeField]
        private bool drawNodeConnections = false;
        [PropertyOrder(3), SerializeField]
        private bool drawGridCellLabels = false;

        public IEnumerable<INode> Nodes => _nodes.Cast<ThreeDNode>();
        public Vector3Int Scale => scale;
        public float CellSize => cellSize;
        
        protected INode[,,] _nodes;
        private Vector3 _previousScale;

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
            if(!InGridBounds(x, y, z)) return;
            _nodes[x, y, z] = value;
        }

        public void SetNode(Vector3 worldPosition, INode value)
        {
            GridPosFromWorldPos(worldPosition, out int x, out int y, out int z);
            SetNode(x, y, z, value);
        }

        public INode GetNode(int x, int y, int z)
        {
            if(InGridBounds(x, y, z)) return _nodes[x, y, z];
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
        protected abstract void Generate();
        
        protected void Generate(Func<Vector3Int, INode> createNode)
        {
            _nodes = new INode[(int)Scale.x, (int)Scale.y, (int)Scale.z];
            for (int x = 0; x < _nodes.GetLength(0); x++)
            {
                for (int y = 0; y < _nodes.GetLength(1); y++)
                {
                    for (int z = 0; z < _nodes.GetLength(2); z++)
                    {
                        INode gridObject = createNode(new Vector3Int(x, y, z));
                        _nodes[x, y, z] = gridObject;
                    }
                }
            }
        }

        protected virtual void DrawGridGizmos()
        {
            // Can we draw right now?
            bool canDraw = 
                (drawGrid || drawNodes || drawGridCellLabels || drawNodeConnections) 
                && _nodes != null;
            if (!canDraw) return;
            
            // Draw the cells
            foreach (INode node in Nodes)
            {
                if (node == null) continue;
                
                // Convert to gizmo space to grid local space...
                Quaternion cellRot = transform.rotation;
                Vector3 cellPos = WorldPosFromGridPos(node.GridPosition);
                Vector3 cellScale = new Vector3(1, 1, 1) * cellSize;

                if (_nodes.GetLength(1) == 1) // 2D
                {
                    cellScale = new Vector3(1, 0, 1) * cellSize;
                    cellPos.y -= cellSize / 2;
                }

                var cellMatrix = Matrix4x4.TRS(cellPos, cellRot, cellScale);
                Gizmos.matrix = cellMatrix;
                Handles.matrix = cellMatrix;
                
                // Draw cells and labels
                if (drawGrid) DrawGridCell(node);
                if (drawGridCellLabels && cellPos.IsInViewOfSceneCamera(35)) DrawGridCellLabel(node);
                
                // Convert to gizmo space to node local space...
                Vector3 nodePos = node.WorldPosition;

                if (_nodes.GetLength(1) == 1) // 2D
                {
                    nodePos.y -= cellSize / 2;
                }

                var nodeMatrix = Matrix4x4.TRS(nodePos, cellRot, cellScale);
                Gizmos.matrix = nodeMatrix;
                
                // Draw nodes and node connections
                if (drawNodes) DrawGridNode(node);
                if (drawNodeConnections) DrawGridNodeConnections(node);
            }
        }

        /// <summary>
        /// Draws grid cell in current cell local space.
        /// </summary>
        protected virtual void DrawGridCell(INode cellNode)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        /// <summary>
        /// Draws grid cell label in node local space.
        /// </summary>
        protected virtual void DrawGridCellLabel(INode cellNode)
        {
            // Draw cell position label...
            Handles.Label(Vector3.zero, cellNode.GridPosition.ToString(), style: new GUIStyle()
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState()
                {
                    textColor = Color.green
                }
            });
        }
        
        /// <summary>
        /// Draws grid node in node local space.
        /// </summary>
        protected virtual void DrawGridNode(INode node)
        {
            var blueColor = Color.blue;
            Gizmos.color = new Color(blueColor.r, blueColor.g, blueColor.b, 0.15f);
            Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.1f);
        }

        /// <summary>
        /// Draws grid node connections in node local space.
        /// </summary>
        protected virtual void DrawGridNodeConnections(INode node)
        {
            Gizmos.color = Color.gray;
            foreach (INode nodeNeighbor in node.Neighbors)
            {
                Gizmos.DrawRay(
                    from: Vector3.zero,
                    direction: (nodeNeighbor.WorldPosition - node.WorldPosition) * CellSize * 0.5f);
            }
        }
        
        private void Update()
        {
            EditorUpdate();
        }
        
#if UNITY_EDITOR 
        private void EditorUpdate()
        {
            // If nodes are null we need to generate
            if (_nodes == null) Generate();
            
            // Listen for ctrl pressed to scale cell size
            ListenForCtrlPressed();
            
            // Keep scale synced between transform and the grids scale property
            ListenForScaleChanges();
        }

        private void ListenForCtrlPressed()
        {
            Event e = Event.current;
            if (e != null && (e.keyCode != KeyCode.LeftControl || e.keyCode != KeyCode.RightControl))
            {
                OnCtrlPressed();
            }
        }

        private void ListenForScaleChanges()
        {
            // transform scale changed
            if (transform.hasChanged)
            {
                if (transform.localScale != Scale) OnTransformScaleChanged();
                transform.hasChanged = false;
            }
            // grid inspector scale value changed
            else if (transform.localScale != Scale) OnGridScaleChanged();
        }

        private void OnCtrlPressed()
        {
            UpdateCellSizeFromScaleDelta();
        }

        private void UpdateCellSizeFromScaleDelta()
        {
            float scaleDelta = _previousScale.magnitude - scale.magnitude;
            cellSize += scaleDelta;
            transform.localScale = _previousScale;
        }

        private void OnGridScaleChanged()
        {
            scale.Clamp(Vector3Int.one, max: Vector3Int.one * int.MaxValue);
            transform.localScale = scale;
            Generate();
        }

        private void OnTransformScaleChanged()
        {
            Vector3 transformLocalScale = transform.localScale;
            transformLocalScale.Snap(1);
            transformLocalScale.Clamp(Vector3.one, max: Vector3.one * int.MaxValue);
            
            transform.localScale = transformLocalScale;
            scale = new Vector3Int((int)transformLocalScale.x, (int)transformLocalScale.y, (int)transformLocalScale.z);
            scale.Clamp(Vector3Int.one, max: Vector3Int.one * int.MaxValue);
            
            Generate();
        }
        
        private void OnDrawGizmos()
        {
            DrawGridGizmos();
        }
#else
        private void EditorUpdate()
        {
            return;
        }
#endif 
    }
}