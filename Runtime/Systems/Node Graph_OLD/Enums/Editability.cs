using System;

namespace Konfus.Systems.Graph.Enums
{
    [Flags]
    public enum Editability
    {
        Unspecified = 0,
        None = 1,
        Inspector = 2,
        NodeView = 4,
        BothViews = Inspector | NodeView
    };
}