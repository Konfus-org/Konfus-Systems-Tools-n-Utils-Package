using System;
using System.Collections.Generic;
using Konfus.Systems.Node_Graph;

namespace Konfus.Tools.NodeGraphEditor
{
    [Serializable]
    public class CopyPasteHelper
    {
        public List<JsonElement> copiedNodes = new();

        public List<JsonElement> copiedGroups = new();

        public List<JsonElement> copiedEdges = new();
    }
}