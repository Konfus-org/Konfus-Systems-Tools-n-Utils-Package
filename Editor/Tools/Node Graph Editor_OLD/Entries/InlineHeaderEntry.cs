namespace Konfus.Tools.Graph_Editor.Editor.Models
{
    /// <summary>
    /// Allows us to display simple headers within the same level and without encapsulated entries
    /// </summary>
    public class InlineHeaderEntry : SearchTreeEntry
    {
        public InlineHeaderEntry(string content) : base(content, AlwaysEnabled)
        {
        }
    }
}