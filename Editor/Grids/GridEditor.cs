using System;
using Konfus.Grids;
using Konfus.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Grids
{
    [CustomEditor(typeof(GridBase), true)]
    internal class GridEditor : UnityEditor.Editor
    {
        private static GridBase? _grid;
        private Texture2D? _gridIcon;

        private void Awake()
        {
            _gridIcon = Resources.Load<Texture2D>("GridIcon");
            Initialize();
        }

        private void Reset()
        {
            _grid = (GridBase)target;
            _grid.Generate();
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnSceneGUI()
        {
            if (!_grid) return;
            DrawGridInteractionHandles(_grid);
            SynchronizeGridScaleAndTransformScaleChanges();
        }

        private void OnValidate()
        {
            Initialize();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawEditorInspectorGui();
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawGridVisualizationGizmos(GridBase grid, GizmoType state)
        {
            if (!_grid) return;

            bool drawGrid = grid.DrawGridCells;
            bool drawNodes = grid.DrawNodes;
            bool drawGridCellLabels = grid.DrawNodeLabels;
            bool drawNodeConnections = grid.DrawNodeConnections;

            // Can we draw right now?
            bool canDraw = drawGrid || drawNodes || drawGridCellLabels || drawNodeConnections;
            if (!canDraw) return;

            // Draw the cells
            foreach (INode node in grid.Nodes)
            {
                if (node == null) continue;

                // Convert to gizmo space to grid local space...
                Quaternion cellRot = grid.transform.rotation;
                Vector3 cellPos = grid.WorldPosFromGridPos(node.GridPosition);
                Vector3 cellScale = new Vector3(1, 1, 1) * grid.CellSize;

                if (grid.NodesXYZ.GetLength(1) == 1) // 2D
                {
                    cellScale = new Vector3(1, 0, 1) * grid.CellSize;
                    cellPos.y -= grid.CellSize / 2;
                }

                Matrix4x4 cellMatrix = Matrix4x4.TRS(cellPos, cellRot, cellScale);
                Gizmos.matrix = cellMatrix;
                Handles.matrix = cellMatrix;

                // Draw cells and labels
                if (drawGrid) DrawGridCell(node, state);
                if (drawGridCellLabels && cellPos.IsInViewOfSceneCamera(grid.CellSize * 24)) DrawNodeLabel(node, state);

                // Convert to gizmo space to node local space...
                Vector3 nodePos = node.WorldPosition;

                if (grid.NodesXYZ.GetLength(1) == 1) // 2D
                    nodePos.y -= grid.CellSize / 2;

                Matrix4x4 nodeMatrix = Matrix4x4.TRS(nodePos, cellRot, cellScale);
                Gizmos.matrix = nodeMatrix;
                Handles.matrix = nodeMatrix;

                // Draw nodes and node connections
                if (drawNodes) DrawGridNode(node, state);
                if (!drawNodeConnections) continue;

                foreach (INode nodeNeighbor in node.Neighbors ?? Array.Empty<INode>())
                {
                    DrawGridNodeConnection(node, nodeNeighbor, state);
                }
            }
        }

        private static void DrawGridCell(INode _, GizmoType state)
        {
            Gizmos.color = new Color(Color.white.r, Color.white.g, Color.white.b,
                state.HasFlag(GizmoType.Selected) ? 1 : 0.15f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        private static void DrawNodeLabel(INode node, GizmoType state)
        {
            var color = new Color(node.DebugColor.r, node.DebugColor.g, node.DebugColor.b,
                state.HasFlag(GizmoType.Selected) ? 1 : 0.15f);
            color = color.Invert();
            Handles.Label(Vector3.back * 0.25f, node.GridPosition.ToString(), new GUIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState { textColor = color }
            });
        }

        private static void DrawGridNode(INode node, GizmoType state)
        {
            Gizmos.color = new Color(node.DebugColor.r, node.DebugColor.g, node.DebugColor.b,
                state.HasFlag(GizmoType.Selected) ? 1 : 0.15f);
            Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.1f);
        }

        private static void DrawGridNodeConnection(INode node, INode nodeNeighbor, GizmoType state)
        {
            Gizmos.color = new Color(node.DebugColor.r, node.DebugColor.g, node.DebugColor.b,
                state.HasFlag(GizmoType.Selected) ? 1 : 0.15f);
            Gizmos.DrawRay(
                Vector3.zero,
                (nodeNeighbor.WorldPosition - node.WorldPosition).normalized * 0.5f);
        }

        private void DrawEditorInspectorGui()
        {
            DrawIcon();
            if (!_grid) return;

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate")) _grid.Generate();
        }

        private void DrawIcon()
        {
            // Set icon
            var grid = (GridBase)target;
            EditorGUIUtility.SetIconForObject(grid, _gridIcon);
        }

        private void DrawGridInteractionHandles(GridBase grid)
        {
            Handles.color = Color.yellow;
            Handles.DrawLine(grid.transform.position, grid.transform.position + grid.transform.up * grid.CellSize / 5,
                0.4f);
            float newCellSize = Handles.ScaleValueHandle(
                grid.CellSize,
                grid.transform.position + grid.transform.up * grid.CellSize / 5,
                grid.transform.rotation,
                (Camera.current.transform.position - grid.transform.position).magnitude / 10,
                Handles.CubeHandleCap,
                1);
            grid.SetCellSize(newCellSize);
        }

        private void Initialize()
        {
            _grid = (GridBase)target;
            _grid.Generate();
        }

        private void SynchronizeGridScaleAndTransformScaleChanges()
        {
            if (!_grid) return;

            // transform scale changed
            if (_grid.transform.hasChanged)
            {
                if (_grid.transform.localScale != _grid.Scale) SyncGridScaleWithTransformScale();
                _grid.transform.hasChanged = false;
            }
            // grid inspector scale value changed
            else if (_grid.transform.localScale != _grid.Scale) SyncTransformScaleWithGridScale();
        }

        private void SyncGridScaleWithTransformScale()
        {
            if (!_grid) return;

            Vector3 transformLocalScale = _grid.transform.localScale;
            transformLocalScale.Snap(1);
            transformLocalScale.Clamp(Vector3.one, Vector3.one * int.MaxValue);
            _grid.transform.localScale = transformLocalScale;
            _grid.SetScale(new Vector3Int((int)transformLocalScale.x, (int)transformLocalScale.y,
                (int)transformLocalScale.z));
            _grid.Generate();
        }

        private void SyncTransformScaleWithGridScale()
        {
            if (!_grid) return;

            Vector3Int newScale = _grid.Scale;
            newScale.Clamp(Vector3Int.one, Vector3Int.one * int.MaxValue);
            _grid.SetScale(newScale);
            _grid.transform.localScale = _grid.Scale;
            _grid.Generate();
        }
    }
}