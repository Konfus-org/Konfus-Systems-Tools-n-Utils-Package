using Konfus.Tools.Graph_Editor.Editor.Controllers;

namespace Konfus.Tools.Graph_Editor.Editor.Interfaces
{
    public interface IUtilityNode
    {
        bool ShouldColorizeBackground();
        bool CreateInspectorUI();
        bool CreateNameUI();
        void Initialize(NodeController nodeController);
    }
}