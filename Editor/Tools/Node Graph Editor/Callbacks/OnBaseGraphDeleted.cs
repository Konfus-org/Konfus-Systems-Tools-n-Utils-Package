using Konfus.Systems.Node_Graph;
using UnityEditor;
using UnityEngine;

namespace Konfus.Tools.NodeGraphEditor
{
    [ExecuteAlways]
    public class DeleteCallback : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (Object obj in objects)
                if (obj is Graph b)
                {
                    foreach (GraphWindow graphWindow in Resources.FindObjectsOfTypeAll<GraphWindow>())
                        graphWindow.OnGraphDeleted();

                    b.OnAssetDeleted();
                }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}