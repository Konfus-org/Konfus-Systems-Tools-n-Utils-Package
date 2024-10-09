using UnityEngine;

namespace Konfus.Utility.Attributes
{
    /// <summary>
    /// Conditionally Show/Hide field in inspector, based on some other field or property value
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class ScenePickerAttribute : PropertyAttribute { }
}
