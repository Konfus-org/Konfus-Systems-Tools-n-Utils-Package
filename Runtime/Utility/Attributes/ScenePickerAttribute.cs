using UnityEngine;

namespace Konfus.Utility.Attributes
{
    /// <summary>
    /// Creates dropdown to pick a scene path from all scenes available from build settings
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class ScenePickerAttribute : PropertyAttribute { }
}
