using Konfus.Systems.Grids;
using Konfus.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Grids
{
    [CustomEditor(typeof(GridBase), editorForChildClasses: true)]
    public class GridEditor : UnityEditor.Editor
    {
        protected static GridBase Grid { get; private set; }
        
        private static GridEditor _instance;

        private Texture2D _gridIcon;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawEditorInspectorGui();
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawGridVisualizationGizmos(GridBase grid, GizmoType state)
        {
            bool drawGrid = grid.DrawGridCells;
            bool drawNodes = grid.DrawNodes;
            bool drawGridCellLabels = grid.DrawNodeLabels;
            bool drawNodeConnections = grid.DrawNodeConnections;
            
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

        private void DrawGridCell(INode cellNode, GizmoType state)
        {
            Gizmos.color = new Color(Color.white.r, Color.white.g, Color.white.b, state.HasFlag(GizmoType.Selected) ? 1 : 0.15f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        private void DrawNodeLabel(INode node, GizmoType state)
        {
            var color = new Color(node.DebugColor.r, node.DebugColor.g, node.DebugColor.b, state.HasFlag(GizmoType.Selected) ? 1 : 0.15f);
            color = color.Invert();
            Handles.Label(Vector3.back * 0.25f, node.GridPosition.ToString(), style: new GUIStyle()
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState() { textColor = color }
            });
        }
        
        private void DrawGridNode(INode node, GizmoType state)
        {
            Gizmos.color = new Color(node.DebugColor.r, node.DebugColor.g, node.DebugColor.b, state.HasFlag(GizmoType.Selected) ? 1 : 0.15f);
            Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.1f);
        }

        private void DrawGridNodeConnection(INode node, INode nodeNeighbor, GizmoType state)
        {
            Gizmos.color = new Color(node.DebugColor.r, node.DebugColor.g, node.DebugColor.b, state.HasFlag(GizmoType.Selected) ? 1 : 0.15f);
            Gizmos.DrawRay(
                from: Vector3.zero,
                direction: (nodeNeighbor.WorldPosition - node.WorldPosition).normalized * 0.5f);
        }
        
        private void DrawEditorInspectorGui()
        {
            DrawIcon();

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate"))
            {
                Grid.Generate();
            }
        }

        private void DrawIcon()
        {
            // Set icon
            var sensor = (GridBase)target;
            EditorGUIUtility.SetIconForObject(sensor, _gridIcon);
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
            _gridIcon = Resources.Load<Texture2D>("GridIcon");
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
        }
        
        private void OnValidate()
        {
            Initialize();
        }

        private void Initialize()
        {
            _instance = this;
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
