using System;
using Konfus.Systems.Graph;
using Konfus.Tools.Graph_Editor.Editor.Controllers;
using Konfus.Tools.Graph_Editor.Editor.Helpers;
using Konfus.Tools.Graph_Editor.Editor.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Tools.Graph_Editor.Views
{
    /// <summary>
    /// The actual editor window for our graph.
    /// Contains a key down workaround to prevent an issue where key down events are passing through elements instead of bein received.
    /// https://forum.unity.com/threads/capturing-keydownevents-in-editorwindow-and-focus.762155/
    /// </summary>
    public class GraphWindow : EditorWindow
    {
        private KeyCode lastKeyCode;
        private EventModifiers lastModifiers;
        private EventType eventType;
        private GraphController graphController;
        private PlayModeStateChange lastState;

        public static event Action<Event> OnGlobalKeyDown;

        [NonSerialized] private static GraphWindow window = null;
        [NonSerialized] private static bool loadRequested = false;

        private static GraphWindow Window
        {
            get
            {
                CacheWindow();
                return window;
            }
        }

        private static void CacheWindow()
        {
            if (window == null)
            {
                window = GetWindow<GraphWindow>(nameof(GraphWindow));
                window.wantsMouseMove = true;
                window.Show();
            }
        }

        [MenuItem(GraphSettings.menuItemBase + nameof(GraphWindow))]
        private static void Initialize()
        {
            CacheWindow();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= LogPlayModeState;
            EditorApplication.playModeStateChanged += LogPlayModeState;
            GlobalKeyEventHandler.OnKeyEvent -= HandleGlobalKeyPressEvents;
            GlobalKeyEventHandler.OnKeyEvent += HandleGlobalKeyPressEvents;
        }

        private void HandleGlobalKeyPressEvents(Event evt)
        {
            if (evt.isKey && mouseOverWindow == this && hasFocus)
            {
                if (lastKeyCode != evt.keyCode || lastModifiers != evt.modifiers)
                {
                    lastModifiers = evt.modifiers;
                    lastKeyCode = evt.keyCode;
                    eventType = evt.type;
                    OnGlobalKeyDown?.Invoke(evt);
                }

                if (evt.type == EventType.KeyUp)
                {
                    lastKeyCode = KeyCode.None;
                    lastModifiers = EventModifiers.None;
                }
            }
        }

        public void LogPlayModeState(PlayModeStateChange state)
        {
            if (lastState == PlayModeStateChange.ExitingPlayMode && state == PlayModeStateChange.EnteredEditMode)
                graphController?.Reload();
            lastState = state;
        }

        private void OnGUI()
        {
            graphController?.Draw();
        }

        private void OnDisable()
        {
            graphController?.Disable();
            GlobalKeyEventHandler.OnKeyEvent -= HandleGlobalKeyPressEvents;
            EditorApplication.playModeStateChanged -= LogPlayModeState;
            loadRequested = false;
        }

        public static void LoadGraph(Graph graph = null)
        {
            if (graph != null)
                GraphSettings.LastOpenedGraph = graph;
            else
                graph = GraphSettings.LastOpenedGraph;

            Window.graphController.OpenGraphExternal(graph);
            loadRequested = true;
        }

        private void CreateGUI()
        {
            VisualElement uxmlRoot = GraphSettings.graphDocument.CloneTree();
            rootVisualElement.Add(uxmlRoot);
            uxmlRoot.StretchToParentSize();

            graphController = new GraphController(uxmlRoot, rootVisualElement);
            rootVisualElement.styleSheets.Add(GraphSettings.graphStylesheetVariables);
            rootVisualElement.styleSheets.Add(GraphSettings.graphStylesheet);

            // add potential custom stylesheet
            if (GraphSettingsSingleton.Settings.customStylesheet != null)
                rootVisualElement.styleSheets.Add(GraphSettingsSingleton.Settings.customStylesheet);

            // delay loading the last graph to the next frame
            // otherwise this method will be called before loadRequested could be set
            rootVisualElement.schedule.Execute(() =>
            {
                if (!loadRequested) LoadGraph();
                loadRequested = false;
            });
        }
    }
}