using UnityEditor;
using UnityEngine;

namespace Konfus.Tools.Replacer
{
    public class Replacer : EditorWindow
    {
        [SerializeField] private GameObject prefab;
        [MenuItem("GameObject/Replace With Prefab", false, 49)]
        private static void CreateReplaceWithPrefabWindow()
        {
            var replacerWindow = GetWindow<Replacer>();
            
            // Set title
            replacerWindow.titleContent.text = "GameObject Replacer"; 
            
            // Set start position and size
            const int width = 320;
            const int height = 200;
            replacerWindow.position = new Rect(replacerWindow.position.x, replacerWindow.position.y, width, height);
        }

        private void OnGUI()
        {
            // Create prefab field
            prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
            
            // Create the replace button and its logic
            if (GUILayout.Button("Replace"))
            {
                GameObject[] selection = Selection.gameObjects;

                for (int i = selection.Length - 1; i >= 0; --i)
                {
                    GameObject selected = selection[i];
                    PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(prefab);
                    GameObject newObject;

                    if (prefabType == PrefabAssetType.Regular)
                    {
                        newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    }
                    else
                    {
                        newObject = Instantiate(prefab);
                        newObject.name = prefab.name;
                    }

                    if (newObject == null)
                    {
                        Debug.LogError("Error instantiating prefab");
                        break;
                    }

                    Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefabs");
                    newObject.transform.parent = selected.transform.parent;
                    newObject.transform.localPosition = selected.transform.localPosition;
                    newObject.transform.localRotation = selected.transform.localRotation;
                    newObject.transform.localScale = selected.transform.localScale;
                    newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                    Undo.DestroyObjectImmediate(selected);
                }
            }

            GUI.enabled = false;
            EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
        }
    }
}