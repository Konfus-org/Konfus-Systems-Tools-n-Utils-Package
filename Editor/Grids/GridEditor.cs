using Konfus.Systems.Grids;
using Konfus.Utility.Extensions;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Grid = Konfus.Systems.Grids.Grid;

namespace Konfus.Editor.Grids
{
    [CustomEditor(typeof(Grid), editorForChildClasses: true)]
    public class GridEditor : UnityEditor.Editor
    {
        private static GridEditor _instance;
        
        private static bool _drawGrid = true;
        private static bool _drawNodes = false;
        private static bool _drawNodeConnections = false;
        private static bool _drawGridCellLabels = false;
        
        private Grid _grid;
        private Vector3 _previousScale;
        
        public override void OnInspectorGUI()
        {
            DrawGui();
            base.OnInspectorGUI();
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
                    direction: (nodeNeighbor.WorldPosition - node.WorldPosition) * (_grid.cellSize * 0.5f));
            }
        }

        /// <summary>
        /// Draws editor settings and internal grid settings (cellsize and scale)
        /// Override to draw custom gui.
        /// </summary>
        protected virtual void DrawGui()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            _grid.cellSize = EditorGUILayout.FloatField("Cell Size", _grid.cellSize);
            _grid.scale = EditorGUILayout.Vector3IntField("Scale", _grid.scale);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            _drawGrid = EditorGUILayout.Toggle("Draw Grid", _drawGrid);
            _drawNodes = EditorGUILayout.Toggle("Draw Nodes", _drawNodes);
            _drawNodeConnections = EditorGUILayout.Toggle("Draw Node Connections", _drawNodeConnections);
            _drawGridCellLabels = EditorGUILayout.Toggle("Draw Cell Labels", _drawGridCellLabels);
        }
        
        [DrawGizmo(GizmoType.Active)]
        private static void DrawGridGizmos(Grid grid, GizmoType gizmoType)
        {
            bool drawGrid = _drawGrid;
            bool drawNodes = _drawNodes;
            bool drawGridCellLabels = _drawGridCellLabels;
            bool drawNodeConnections = _drawNodeConnections;
            
            // Can we draw right now?
            bool canDraw = (drawGrid || drawNodes || drawGridCellLabels || drawNodeConnections) && grid.nodes != null;
            if (!canDraw) return;
            
            // Draw the cells
            foreach (INode node in grid.Nodes)
            {
                if (node == null) continue;
                
                // Convert to gizmo space to grid local space...
                Quaternion cellRot = grid.transform.rotation;
                Vector3 cellPos = grid.WorldPosFromGridPos(node.GridPosition);
                Vector3 cellScale = new Vector3(1, 1, 1) * grid.CellSize;

                if (grid.nodes.GetLength(1) == 1) // 2D
                {
                    cellScale = new Vector3(1, 0, 1) * grid.CellSize;
                    cellPos.y -= grid.CellSize / 2;
                }

                var cellMatrix = Matrix4x4.TRS(cellPos, cellRot, cellScale);
                Gizmos.matrix = cellMatrix;
                Handles.matrix = cellMatrix;
                
                // Draw cells and labels
                if (drawGrid) _instance.DrawGridCell(node);
                if (drawGridCellLabels && cellPos.IsInViewOfSceneCamera(10)) _instance.DrawGridCellLabel(node);
                
                // Convert to gizmo space to node local space...
                Vector3 nodePos = node.WorldPosition;

                if (grid.nodes.GetLength(1) == 1) // 2D
                {
                    nodePos.y -= grid.cellSize / 2;
                }

                var nodeMatrix = Matrix4x4.TRS(nodePos, cellRot, cellScale);
                Gizmos.matrix = nodeMatrix;
                Handles.matrix = nodeMatrix;
                
                // Draw nodes and node connections
                if (drawNodes) _instance.DrawGridNode(node);
                if (drawNodeConnections) _instance.DrawGridNodeConnections(node);
            }
        }

        private void OnEnable()
        {
            _instance = this;
            _grid = (Grid)target;
        }
        
        private void OnValidate()
        {
            _grid.Generate();
        }

        private void OnSceneGUI()
        {
            ListenForScaleChanges();
        }
        
        private void ListenForScaleChanges()
        {
            // transform scale changed
            if (_grid.transform.hasChanged)
            {
                if (_grid.transform.localScale != _grid.Scale) OnTransformScaleChanged();
                _grid.transform.hasChanged = false;
            }
            // grid inspector scale value changed
            else if (_grid.transform.localScale != _grid.Scale) OnGridScaleChanged();
        }

        private void OnTransformScaleChanged()
        {
            _previousScale = _grid.transform.localScale;
            
            // if we are pressing shift update cell size
            //if ((Event.current.modifiers & EventModifiers.Shift) != 0) UpdateCellSizeFromTransformScaleDelta();
            // else update scale
            /*else*/ UpdateGridScaleFromTransformScale();

            // Finally re-generate the grid to update it...
            _grid.Generate();
        }
        
        private void OnGridScaleChanged()
        {
            _grid.scale.Clamp(Vector3Int.one, max: Vector3Int.one * int.MaxValue);
            _grid.transform.localScale = _grid.scale;
            _grid.Generate();
        }

        // A pain in the butt, maybe later we will do this...
        /*private void UpdateCellSizeFromTransformScaleDelta()
        {
            var scaleDelta = _previousScale.magnitude - _grid.transform.localScale.magnitude;
            _grid.cellSize += scaleDelta / 10;
            _grid.transform.localScale = _previousScale;
        }*/

        private void UpdateGridScaleFromTransformScale()
        {
            Vector3 transformLocalScale = _grid.transform.localScale;
            transformLocalScale.Snap(1);
            transformLocalScale.Clamp(min: Vector3.one, max: Vector3.one * int.MaxValue);
        
            _grid.transform.localScale = transformLocalScale;
            _grid.scale = new Vector3Int((int)transformLocalScale.x, (int)transformLocalScale.y, (int)transformLocalScale.z);
            _grid.scale.Clamp(min: Vector3Int.one, max: Vector3Int.one * int.MaxValue);
        }
    }
}
