using Konfus.Systems.Grids;
using Konfus.Utility.Extensions;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Grids
{
    [CustomEditor(typeof(GridBase), editorForChildClasses: true)]
    public class GridEditor : UnityEditor.Editor
    {
        protected static GridBase Grid { get; private set; }
        
        private static GridEditor _instance;
        private static bool _drawGrid = true;
        private static bool _drawGridNodes = false;
        private static bool _drawGridNodeLabels = false;
        private static bool _drawGridNodeConnections = false;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawInspectorGui();
        }

        /// <summary>
        /// Draws grid cell in current cell local space.
        /// </summary>
        protected virtual void DrawGridCell(INode cellNode, GizmoType state)
        {
            var color = Color.white;
            Gizmos.color = new Color(color.r, color.g, color.b, state.HasFlag(GizmoType.Selected) ? 1 : 0.15f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        /// <summary>
        /// Draws grid cell label in node local space.
        /// </summary>
        protected virtual void DrawNodeLabel(INode node, GizmoType state)
        {
            var color = Color.green;
            color = new Color(color.r, color.g, color.b, state.HasFlag(GizmoType.Selected) ? 1 : 0.15f);
            Handles.Label(Vector3.back * 0.25f, node.GridPosition.ToString(), style: new GUIStyle()
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState() { textColor = color }
            });
        }
        
        /// <summary>
        /// Draws grid node in node local space.
        /// </summary>
        protected virtual void DrawGridNode(INode node, GizmoType state)
        {
            var color = Color.blue;
            Gizmos.color = new Color(color.r, color.g, color.b, state.HasFlag(GizmoType.Selected) ? 0.5f : 0.15f);
            Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.1f);
        }

        /// <summary>
        /// Draws grid node connections in node local space.
        /// </summary>
        protected virtual void DrawGridNodeConnection(INode node, INode nodeNeighbor, GizmoType state)
        {
            var color = Color.blue;
            Gizmos.color = new Color(color.r, color.g, color.b, state.HasFlag(GizmoType.Selected) ? 0.35f : 0.15f);
            Gizmos.DrawRay(
                from: Vector3.zero,
                direction: (nodeNeighbor.WorldPosition - node.WorldPosition).normalized * 0.5f);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected | GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        private static void DrawGridVisualizationGizmos(GridBase grid, GizmoType state)
        {
            bool drawGrid = _drawGrid;
            bool drawNodes = _drawGridNodes;
            bool drawGridCellLabels = _drawGridNodeLabels;
            bool drawNodeConnections = _drawGridNodeConnections;
            
            // Can we draw right now?
            bool canDraw = (drawGrid || drawNodes || drawGridCellLabels || drawNodeConnections) && grid.NodesXYZ != null;
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

                var cellMatrix = Matrix4x4.TRS(cellPos, cellRot, cellScale);
                Gizmos.matrix = cellMatrix;
                Handles.matrix = cellMatrix;
                
                // Draw cells and labels
                if (drawGrid) _instance.DrawGridCell(node, state);
                if (drawGridCellLabels && cellPos.IsInViewOfSceneCamera(grid.CellSize * 24)) _instance.DrawNodeLabel(node, state);
                
                // Convert to gizmo space to node local space...
                Vector3 nodePos = node.WorldPosition;

                if (grid.NodesXYZ.GetLength(1) == 1) // 2D
                {
                    nodePos.y -= grid.CellSize / 2;
                }

                var nodeMatrix = Matrix4x4.TRS(nodePos, cellRot, cellScale);
                Gizmos.matrix = nodeMatrix;
                Handles.matrix = nodeMatrix;
                
                // Draw nodes and node connections
                if (drawNodes) _instance.DrawGridNode(node, state);
                if (!drawNodeConnections) continue;
                
                foreach (INode nodeNeighbor in node.Neighbors)
                {
                    _instance.DrawGridNodeConnection(node, nodeNeighbor, state);
                }
            }
        }

        private void DrawInspectorGui()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            _drawGrid = EditorGUILayout.Toggle("Draw Grid", _drawGrid);
            _drawGridNodes = EditorGUILayout.Toggle("Draw Nodes", _drawGridNodes);
            _drawGridNodeConnections = EditorGUILayout.Toggle("Draw Node Connections", _drawGridNodeConnections);
            _drawGridNodeLabels = EditorGUILayout.Toggle("Draw Node Labels", _drawGridNodeLabels);

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate"))
            {
                Grid.Generate();
            }
        }

        private void OnSceneGUI()
        {
            DrawGridInteractionHandles(Grid);
            SynchronizeGridScaleAndTransformScaleChanges();
        }
        
        private void DrawGridInteractionHandles(GridBase grid)
        {
            Handles.color = Color.yellow;
            Handles.DrawLine(grid.transform.position, grid.transform.position + grid.transform.up * grid.CellSize/5, 0.4f); 
            var newCellSize = Handles.ScaleValueHandle(
                value: grid.CellSize, 
                position: grid.transform.position + grid.transform.up * grid.CellSize/5,
                rotation: grid.transform.rotation, 
                size: (Camera.current.transform.position - grid.transform.position).magnitude/10, 
                capFunction: Handles.CubeHandleCap, 
                snap: 1); 
            grid.SetCellSize(newCellSize);
        }
        private void Awake()
        {
            _instance = this;
        }

        private void OnEnable()
        {
            Grid = (GridBase)target;
            Grid.Generate();
        }
        
        private void OnValidate()
        {
            Grid = (GridBase)target;
            Grid.Generate();
        }

        private void Reset()
        {
            Grid = (GridBase)target;
            Grid.Generate();
        }
        
        private void SynchronizeGridScaleAndTransformScaleChanges()
        {
            // transform scale changed
            if (Grid.transform.hasChanged)
            {
                if (Grid.transform.localScale != Grid.Scale) SyncGridScaleWithTransformScale();
                Grid.transform.hasChanged = false;
            }
            // grid inspector scale value changed
            else if (Grid.transform.localScale != Grid.Scale) SyncTransformScaleWithGridScale();
        }
        
        private void SyncGridScaleWithTransformScale()
        {
            Vector3 transformLocalScale = Grid.transform.localScale;
            transformLocalScale.Snap(1);
            transformLocalScale.Clamp(min: Vector3.one, max: Vector3.one * int.MaxValue);
            Grid.transform.localScale = transformLocalScale;
            Grid.SetScale(new Vector3Int((int)transformLocalScale.x, (int)transformLocalScale.y, (int)transformLocalScale.z));
            Grid.Generate();
        }
        
        private void SyncTransformScaleWithGridScale()
        {
            Vector3Int newScale = Grid.Scale;
            newScale.Clamp(min: Vector3Int.one, max: Vector3Int.one * int.MaxValue);
            Grid.SetScale(newScale);
            Grid.transform.localScale = Grid.Scale;
            Grid.Generate();
        }
    }
}
