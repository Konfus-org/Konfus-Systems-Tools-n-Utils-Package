using Konfus.Systems.Graph.Attributes;

namespace Konfus.Tools.Graph_Editor.Editor.Serialization.PropertyInfo
{
    public class GraphPropertyInfo : PropertyInfo
    {
        public GraphPropertyInfo(string relativePropertyPath, GraphDisplayAttribute graphDisplay = null) : base(
            relativePropertyPath)
        {
            if (graphDisplay != null)
            {
                this.graphDisplay = graphDisplay;
                hasCustomGraphDisplay = true;
            }
        }

        public bool hasCustomGraphDisplay = false;
        public GroupInfo groupInfo = null;
        public GraphDisplayAttribute graphDisplay = new();
    }
}