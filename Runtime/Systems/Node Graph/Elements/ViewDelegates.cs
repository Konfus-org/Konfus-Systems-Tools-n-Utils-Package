using System;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    public class ViewDelegates
    {
        private readonly Node _node;
        private readonly Func<Rect> _getRect;
        private readonly Action<Rect> _setRect;
        private readonly Action _updateTitle;

        public ViewDelegates(Node node)
        {
            _node = node;
        }

        public ViewDelegates(Node node, Func<Rect> getRect, Action<Rect> setRect, Action updateTitle)
        {
            _node = node;
            _getRect = getRect;
            _setRect = setRect;
            _updateTitle = updateTitle;
        }

        public Func<Rect> GetRect => _getRect ?? (() => _node.initialPosition);
        public Action<Rect> SetRect => _setRect ?? (rect => _node.initialPosition = rect);
        public Action UpdateTitle => _updateTitle ?? (() => { });

        public Vector2 GetPosition()
        {
            return GetRect().position;
        }

        public void SetPosition(Vector2 position)
        {
            SetRect(new Rect(position, GetSize()));
        }

        public Vector2 GetSize()
        {
            return GetRect().size;
        }
        // public void SetSize(Vector2 size) => SetRect(new Rect(GetPosition(), size));
    }
}