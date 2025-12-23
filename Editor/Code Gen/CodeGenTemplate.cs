namespace Konfus.Editor.Code_Gen
{
    public sealed record CodeGenTemplate(
        string Name,
        string Content)
    {
        public string Name { get; } = Name;
        public string Content { get; } = Content;
    }
}