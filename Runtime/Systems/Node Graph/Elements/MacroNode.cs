using System;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    public class MacroNode : SubGraphNode
    {
        public override Color color => new(0, 0, 1, 0.5f);
        public override bool needsInspector => false;

        public static Node InstantiateMacro(Type nodeType, Vector2 position, params object[] args)
        {
            var macro = args[0] as SubGraph;
            var macroNode = CreateFromType(nodeType, position, args) as MacroNode;
            macroNode.SetMacro(macro);
            return macroNode;
        }

        public void SetMacro(SubGraph macro)
        {
            subGraph = macro;
        }
    }
}