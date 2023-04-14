using Konfus.Tools.Graph_Editor.Editor.Settings;
using Konfus.Tools.Graph_Editor.Views.Elements;
using UnityEngine;

namespace Konfus.Tools.Graph_Editor.Views
{
    public class EdgeView : Edge
    {
        public EdgeView() : base()
        {
            EdgeWidth = EdgeWidthUnselected;
        }

        public override int EdgeWidthSelected => GraphSettingsSingleton.Settings.edgeWidthSelected;
        public override int EdgeWidthUnselected => GraphSettingsSingleton.Settings.edgeWidthUnselected;
        public override Color ColorSelected => GraphSettingsSingleton.Settings.colorSelected;
        public override Color ColorUnselected => currentUnselectedColor;

        public Color currentUnselectedColor = GraphSettingsSingleton.Settings.colorUnselected;
    }
}