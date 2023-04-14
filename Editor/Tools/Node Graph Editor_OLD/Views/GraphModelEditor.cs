using Konfus.Systems.Graph;
using Konfus.Tools.Graph_Editor.Editor.Settings;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using Graph = Konfus.Systems.Graph.Graph;

namespace Konfus.Tools.Graph_Editor.Views
{
    [CustomEditor(typeof(Graph))]
    public class GraphModelEditor : UnityEditor.Editor
    {
        private SerializedProperty listProperty;

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();
            inspector.AddToClassList("baseGraphEditor");

            var openGraphButton = new Button(OpenGraphClicked)
                {text = GraphSettingsSingleton.Settings.openGraphButtonText};
            openGraphButton.Add(GraphSettings.LoadButtonIcon);
            inspector.Add(openGraphButton);
            inspector.styleSheets.Add(GraphSettings.graphStylesheetVariables);
            inspector.styleSheets.Add(GraphSettings.graphStylesheet);

            listProperty = serializedObject.FindProperty(nameof(Graph.nodes));
            var listView = new ListView()
            {
                showAddRemoveFooter = false,
                reorderable = false,
                showFoldoutHeader = false,
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                bindingPath = listProperty.propertyPath,
                bindItem = BindItem,
                makeItem = MakeItem
            };
            inspector.Add(listView);

            return inspector;
        }

        private VisualElement MakeItem()
        {
            var itemRow = new VisualElement();
            var fieldLabel = new Label();
            fieldLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            itemRow.style.flexDirection = FlexDirection.Row;
            itemRow.Add(fieldLabel);
            itemRow.SetEnabled(false);
            return itemRow;
        }

        private void BindItem(VisualElement itemRow, int i)
        {
            //serializedObject.Update();
            SerializedProperty prop = listProperty.GetArrayElementAtIndex(i);
            var label = itemRow[0] as Label;
            if (prop != null)
            {
                SerializedProperty propRelative = prop.FindPropertyRelative(Node.nameIdentifier);
                if (propRelative != null) label.text = $"Element {i + 1}: {propRelative.stringValue}";
            }
        }

        private void OpenGraphClicked()
        {
            OpenGraph(target as Graph);
        }

        private static void OpenGraph(Graph graph)
        {
            GraphWindow.LoadGraph(graph);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var baseGraphModel = EditorUtility.InstanceIDToObject(instanceID) as Graph;
            if (baseGraphModel != null)
            {
                OpenGraph(baseGraphModel);
                return true;
            }

            return false;
        }
    }
}