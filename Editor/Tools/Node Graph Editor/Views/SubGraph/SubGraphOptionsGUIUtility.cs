using Konfus.Systems.Node_Graph;
using Konfus.Systems.Node_Graph.Schema;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    public class SubGraphOptionsGUIUtility
    {
        private SerializedObject _subGraphSerialized;
        private SerializedProperty _options;
        private SerializedProperty _displayName;
        private SerializedProperty _renamePolicy;

        public SubGraphOptionsGUIUtility(SubGraph subGraph)
        {
            SubGraph = subGraph;
        }

        public SubGraph SubGraph { get; }

        public SerializedObject SubGraphObject =>
            PropertyUtils.LazyLoad(
                ref _subGraphSerialized,
                () => new SerializedObject(SubGraph)
            );

        public SerializedProperty Options =>
            PropertyUtils.LazyLoad(
                ref _options,
                () => SubGraphObject.FindProperty(SubGraph.OptionsFieldName)
            );

        public SerializedProperty DisplayName =>
            PropertyUtils.LazyLoad(
                ref _displayName,
                () => Options.FindPropertyRelative(SubGraphOptions.DisplayNameFieldName)
            );

        public SerializedProperty RenamePolicy =>
            PropertyUtils.LazyLoad(
                ref _renamePolicy,
                () => Options.FindPropertyRelative(SubGraphOptions.RenamePolicyFieldName)
            );

        public Foldout DrawGUI()
        {
            var optionsFoldout = new Foldout
            {
                text = "SubGraph Configuration"
            };

            PropertyField displayNameField = DrawDisplayNameField(false);
            displayNameField.RegisterCallback<ChangeEvent<string>>(prop =>
            {
                if (string.Equals(prop.previousValue, prop.newValue))
                    return;

                SubGraph.NotifyOptionsChanged();
            });

            PropertyField renamePolicyField = DrawRenamePolicyField(false);
            renamePolicyField.RegisterCallback<ChangeEvent<string>>(e =>
            {
                if (e.previousValue == e.newValue)
                    return;

                SubGraph.NotifyOptionsChanged();
            });

            optionsFoldout.Add(displayNameField);
            optionsFoldout.Add(renamePolicyField);

            optionsFoldout.Bind(SubGraphObject);

            return optionsFoldout;
        }

        public PropertyField DrawDisplayNameField(bool bind = true)
        {
            PropertyField displayNameField = new(DisplayName);

            if (bind) displayNameField.Bind(SubGraphObject);

            return displayNameField;
        }

        public PropertyField DrawRenamePolicyField(bool bind = true)
        {
            PropertyField renamePolicyField = new(RenamePolicy);

            if (bind) renamePolicyField.Bind(SubGraphObject);

            return renamePolicyField;
        }
    }
}