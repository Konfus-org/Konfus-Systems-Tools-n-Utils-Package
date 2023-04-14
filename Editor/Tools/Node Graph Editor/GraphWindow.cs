using System;
using System.Linq;
using Konfus.Systems.Node_Graph;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    [Serializable]
    public abstract class GraphWindow : EditorWindow
    {
        protected GraphView graphView;
        protected VisualElement rootView;

        private readonly string graphWindowStyle = "GraphProcessorStyles/BaseGraphView";

        private bool reloadWorkaround;

        [SerializeField] protected Graph graph;

        public event Action<Graph> graphLoaded;
        public event Action<Graph> graphUnloaded;

        public bool isGraphLoaded => graphView != null && graphView.graph != null;

        public virtual void OnGraphDeleted()
        {
            if (graph != null && graphView != null)
                rootView.Remove(graphView);

            graphView = null;
        }

        public void InitializeGraph(Graph graph)
        {
            if (this.graph != null && graph != this.graph)
            {
                // Save the graph to the disk
                EditorUtility.SetDirty(this.graph);
                AssetDatabase.SaveAssets();
                // Unload the graph
                graphUnloaded?.Invoke(this.graph);
            }

            graphLoaded?.Invoke(graph);
            this.graph = graph;

            if (graphView != null)
                rootView.Remove(graphView);

            //Initialize will provide the BaseGraphView
            InitializeWindow(graph);

            graphView = rootView.Children().FirstOrDefault(e => e is GraphView) as GraphView;

            if (graphView == null)
            {
                Debug.LogError("GraphView has not been added to the BaseGraph root view !");
                return;
            }

            graphView.Initialize(graph);
            graph.Initialize();

            InitializeGraphView(graphView);

            // TOOD: onSceneLinked...

            if (graph.IsLinkedToScene())
                LinkGraphWindowToScene(graph.GetLinkedScene());
            else
                graph.onSceneLinked += LinkGraphWindowToScene;
        }

        protected virtual void InitializeGraphView(GraphView view)
        {
        }

        /// <summary>
        ///     Called by Unity when the window is closed
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        ///     Called by Unity when the window is disabled (happens on domain reload)
        /// </summary>
        protected virtual void OnDisable()
        {
            if (graph != null && graphView != null)
                graphView.SaveGraphToDisk();
        }

        /// <summary>
        ///     Called by Unity when the window is enabled / opened
        /// </summary>
        protected virtual void OnEnable()
        {
            InitializeRootView();

            if (graph != null)
                LoadGraph();
            else
                reloadWorkaround = true;
        }

        protected virtual void Update()
        {
            // Workaround for the Refresh option of the editor window:
            // When Refresh is clicked, OnEnable is called before the serialized data in the
            // editor window is deserialized, causing the graph view to not be loaded
            if (reloadWorkaround && graph != null)
            {
                LoadGraph();
                reloadWorkaround = false;
            }
        }

        protected abstract void InitializeWindow(Graph graph);

        private void LinkGraphWindowToScene(Scene scene)
        {
            EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;

            void CloseWindowWhenSceneIsClosed(Scene closedScene)
            {
                if (scene == closedScene)
                {
                    Close();
                    EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
                }
            }
        }

        private void InitializeRootView()
        {
            rootView = rootVisualElement;

            rootView.name = "graphRootView";

            rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
        }

        private void LoadGraph()
        {
            // We wait for the graph to be initialized
            if (graph.isEnabled)
                InitializeGraph(graph);
            else
                graph.onEnabled += () => InitializeGraph(graph);
        }
    }
}