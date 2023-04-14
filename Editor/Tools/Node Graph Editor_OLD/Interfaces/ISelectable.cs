using UnityEngine;

namespace Konfus.Tools.Graph_Editor.Editor.Interfaces
{
    public interface ISelectable
    {
        ISelector Selector { get; }
        bool Selected { get; set; }
        bool Overlaps(Rect rectangle);
        bool ContainsPoint(Vector2 localPoint);
    }
}