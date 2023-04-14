using System;

namespace Konfus.Systems.Graph.Enums
{
    [Flags]
    public enum DisplayType
    {
        Unspecified = 0,
        Hide = 1,
        Inspector = 2,
        NodeView = 4,
        BothViews = Inspector | NodeView
    };
}