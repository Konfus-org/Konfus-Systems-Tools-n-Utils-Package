using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Extensions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Konfus.Systems.Grid
{
    [ExecuteInEditMode]
    public abstract class GridBase : MonoBehaviour
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
        private bool drawGridCellLabels = false;
        [PropertyOrder(4), SerializeField]
        private Color gridColor = Color.white;
        [PropertyOrder(4), SerializeField]
        private Color gridCellLabelColor = new Color(0, 1, 0.328f);

        public IEnumerable<INode> Nodes => _nodes.Cast<Node>();
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
            x = Mathf.FloorToInt(worldPosition.x / cellSize);
            y = Mathf.FloorToInt(worldPosition.y / cellSize);
            z = Mathf.FloorToInt(worldPosition.z / cellSize);
        }

        public Vector3 GridPosFromWorldPos(Vector3 worldPosition)
        {
            GridPosFromWorldPos(worldPosition, out int x, out int y, out int z);
            return new Vector3(x, y, z);
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

        public INode GetNode(Vector3 worldPosition)
        {
            GridPosFromWorldPos(worldPosition, out int x, out int y, out int z);
            return GetNode(x, y, z);
        }
        
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
            if (!drawGrid || _nodes == null) return;
            
            // Draw the cells
            foreach (INode node in Nodes)
            {
                if (node == null) continue;
                
                // Draw cell...
                Quaternion nodeRot = transform.rotation;
                Vector3 nodePos = WorldPosFromGridPos(node.GetGridPosition());
                Vector3 nodeScale = new Vector3(1, 1, 1) * cellSize;

                if (_nodes.GetLength(1) == 1) // 2D
                {
                    nodeScale = new Vector3(1, 0, 1) * cellSize;
                    nodePos.y -= cellSize / 2;
                }
                
                Gizmos.color = gridColor;
                Gizmos.matrix = Matrix4x4.TRS(nodePos, nodeRot, nodeScale);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

                // Do we want to draw the labels?
                if (!drawGridCellLabels) continue;
                
                // Draw cell position label...
                Vector3 handlePos = nodePos;
                if (!handlePos.IsInViewOfSceneCamera(35)) continue;
                Handles.Label(handlePos, node.GetGridPosition().ToString(), style: new GUIStyle()
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    normal = new GUIStyleState()
                    {
                        textColor = gridCellLabelColor
                    }
                });
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