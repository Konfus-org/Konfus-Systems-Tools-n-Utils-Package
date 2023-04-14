using UnityEngine;

namespace Konfus.Tools.Graph_Editor.Editor.Settings
{
    //[CreateAssetMenu(fileName = nameof(GraphSettingsAsset), menuName = nameof(GraphSettingsAsset), order = 1)]
    public class GraphSettingsAsset : ScriptableObject
    {
        public GraphSettings graphSettings = new();
    }
}