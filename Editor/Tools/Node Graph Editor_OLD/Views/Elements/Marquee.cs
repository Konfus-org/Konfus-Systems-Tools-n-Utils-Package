using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Tools.Graph_Editor.Views.Elements
{
    internal class Marquee : VisualElement
    {
        private Vector2 m_End;
        private Vector2 m_Start;

        internal Marquee()
        {
            AddToClassList("marquee");
            pickingMode = PickingMode.Ignore;
            generateVisualContent = OnGenerateVisualContent;
        }

        internal Vector2 Start
        {
            get => m_Start;
            set
            {
                m_Start = value;
                MarkDirtyRepaint();
            }
        }

        internal Vector2 End
        {
            get => m_End;
            set
            {
                m_End = value;
                MarkDirtyRepaint();
            }
        }

        internal RectangleCoordinates Coordinates
        {
            get => new() {start = m_Start, end = m_End};
            set
            {
                m_Start = value.start;
                m_End = value.end;
                MarkDirtyRepaint();
            }
        }

        internal Rect SelectionRect =>
            new()
            {
                min = new Vector2(Math.Min(m_Start.x, m_End.x), Math.Min(m_Start.y, m_End.y)),
                max = new Vector2(Math.Max(m_Start.x, m_End.x), Math.Max(m_Start.y, m_End.y))
            };

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            Rect selectionRect = SelectionRect;
            Painter2D painter = ctx.painter2D;
            painter.lineWidth = 1.0f;
            painter.strokeColor = Color.white;
            painter.fillColor = Color.gray;
            painter.BeginPath();
            painter.MoveTo(new Vector2(selectionRect.xMin, selectionRect.yMin));
            painter.LineTo(new Vector2(selectionRect.xMax, selectionRect.yMin));
            painter.LineTo(new Vector2(selectionRect.xMax, selectionRect.yMax));
            painter.LineTo(new Vector2(selectionRect.xMin, selectionRect.yMax));
            painter.LineTo(new Vector2(selectionRect.xMin, selectionRect.yMin));
            painter.Stroke();
            painter.Fill();
        }

        internal struct RectangleCoordinates
        {
            internal Vector2 start;
            internal Vector2 end;
        }
    }
}