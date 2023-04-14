using System;

namespace Konfus.Tools.Graph_Editor.Editor.Attributes
{
    /// <summary>
    /// Attribute that can be attached to a class that inherits from ContextMenu to change & customize the main context menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomContextMenuAttribute : Attribute
    {
        public CustomContextMenuAttribute()
        {
        }
    }
}