using Konfus.Systems.Grids;
using Konfus.Utility.Extensions;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Editor.Notes
{
    [CustomEditor(typeof(Note), editorForChildClasses: true)]
    public class NotesEditor : UnityEditor.Editor
    {
        // TODO: Show note in editor...
        
        // TODO: Serialize notes into a folder outside assets folder
    }
}