#if UNITY_2020_1_OR_NEWER
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    public class StickyNoteView : StickyNote
    {
        public GraphView owner;
        public Systems.Node_Graph.StickyNote note;

        private Label titleLabel;
        private ColorField colorField;

        public StickyNoteView()
        {
            fontSize = StickyNoteFontSize.Small;
            theme = StickyNoteTheme.Classic;
        }

        public void Initialize(GraphView graphView, Systems.Node_Graph.StickyNote note)
        {
            this.note = note;
            owner = graphView;

            this.Q<TextField>("title-field").RegisterCallback<ChangeEvent<string>>(e => { note.title = e.newValue; });
            this.Q<TextField>("contents-field")
                .RegisterCallback<ChangeEvent<string>>(e => { note.content = e.newValue; });

            title = note.title;
            contents = note.content;
            SetPosition(note.position);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            if (note != null)
                note.position = newPos;
        }

        public override void OnResized()
        {
            note.position = layout;
        }
    }
}
#endif