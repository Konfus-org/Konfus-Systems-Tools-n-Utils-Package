namespace Konfus.Tools.Graph_Editor.Editor.Serialization
{
    /// <summary>
    /// an NodeReference contains the object value of the original node
    /// and the relative property path
    /// </summary>
    public class NodeReference
    {
        public object nodeData;
        public string relativePropertyPath;
    }
}