using System;
using Codice.CM.SEIDInfo;
using UnityEngine;

namespace Konfus.Utility.Attributes
{
    /// <summary>
    /// Creates dropdown to pick a component of one of the types in the given typeFilter path from the parent game object
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class ComponentPickerAttribute : PropertyAttribute
    {
        public readonly Type[] TypeFilter;

        public ComponentPickerAttribute(Type[] typeFilter = null)
        {
            TypeFilter = typeFilter;
        }
    }
}