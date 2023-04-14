using System;
using System.Collections.Generic;
using Konfus.Systems.Graph;
using Konfus.Tools.Graph_Editor.Editor.Enums;
using Konfus.Tools.Graph_Editor.Editor.Serialization.PropertyInfo;
using Konfus.Tools.Graph_Editor.Editor.Settings;
using Konfus.Tools.Graph_Editor.Views.Elements;
using Konfus.Tools.Graph_Editor.Views.Nodes;
using UnityEditor;
using UnityEngine;

namespace Konfus.Tools.Graph_Editor.Views
{
    public class PortView : BasePort
    {
        public Type type;
        public SerializedProperty boundProperty;
        public List<Type> connectableTypes;

        private Action connectionChangedCallback;
        private Func<Type, Type, bool> isValidConnectionCheck;
        private UnityEngine.Color targetColor;

        public PortView(PortInfo info, SerializedProperty boundProperty, Action connectionChangedCallback = null) :
            base(Orientation.Horizontal, (Direction) (int) info.portDisplay.direction,
                (PortCapacity) (int) info.portDisplay.capacity)
        {
            type = info.fieldType;
            isValidConnectionCheck = info.portDisplay.isValidConnectionCheck;
            connectableTypes = info.connectableTypes;
            this.boundProperty = boundProperty;
            this.connectionChangedCallback = connectionChangedCallback;

            PortColor = DefaultPortColor;
        }

        public override UnityEngine.Color DefaultPortColor => GraphSettingsSingleton.Settings.portColor;
        public override UnityEngine.Color DisabledPortColor => GraphSettingsSingleton.Settings.disabledPortColor;

        public void SetConnectionChangedCallback(Action callback)
        {
            connectionChangedCallback = callback;
            connectionChangedCallback();
        }

        public override void Connect(BaseEdge edge)
        {
            base.Connect(edge);
            if (edge.Input != null && edge.Output != null)
            {
                //Logger.Log("Connect");
                var inputPort = edge.Input as PortView;
                var outputPort = edge.Output as PortView;

                if (outputPort.boundProperty.managedReferenceValue != inputPort.boundProperty.managedReferenceValue)
                {
                    //Logger.Log("Connect: change values");
                    //Undo.RegisterCompleteObjectUndo(outputPort.boundProperty.serializedObject.targetObject, "Add Connection");
                    //outputPort.boundProperty.managedReferenceValue = inputPort.boundProperty.managedReferenceValue;
                    outputPort.boundProperty.managedReferenceId = inputPort.boundProperty.managedReferenceId;
                    //EditorUtility.SetDirty(outputPort.boundProperty.serializedObject.targetObject);
                    outputPort.boundProperty.serializedObject.ApplyModifiedProperties();
                    connectionChangedCallback?.Invoke();
                }

                ColorizeEdgeAndPort(edge as EdgeView);
            }
        }

        public void Reset()
        {
            Debug.Log("Reset");
            // set its value to null = remove reference
            boundProperty.managedReferenceValue = null;
            boundProperty.serializedObject.ApplyModifiedProperties();
            connectionChangedCallback?.Invoke();
        }

        public override bool CanConnectTo(BasePort other, bool ignoreCandidateEdges = true)
        {
            PortView outputPort;
            PortView inputPort;

            if (Direction == Direction.Output)
            {
                outputPort = this;
                inputPort = (PortView) other;
            }
            else
            {
                outputPort = (PortView) other;
                inputPort = this;
            }
            
            if (!isValidConnectionCheck(inputPort.type, outputPort.type)) return false;
            return base.CanConnectTo(other, ignoreCandidateEdges);
        }

        private void ColorizeEdgeAndPort(EdgeView edge)
        {
            Type nodeType = (edge.Input as PortView).type;
            targetColor = Node.GetNodeAttribute(nodeType)?.color ?? targetColor;
            edge.currentUnselectedColor =
                targetColor == default ? GraphSettingsSingleton.Settings.colorUnselected : targetColor;
            edge.InputColor = edge.OutputColor = edge.currentUnselectedColor;

            if (edge.Input != null)
            {
                targetColor = targetColor == default ? edge.Input.DefaultPortColor : targetColor;
                edge.Input.PortColor = targetColor;
                if (edge.Output != null) edge.Output.PortColor = targetColor;
            }
        }

        public override BaseEdge ConnectTo(BasePort other)
        {
            var edge = base.ConnectTo(other) as EdgeView;
            ColorizeEdgeAndPort(edge);
            return edge;
        }
    }
}