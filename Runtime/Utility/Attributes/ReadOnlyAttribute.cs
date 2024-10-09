using System;
using UnityEngine;

namespace Konfus.Utility.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ReadOnlyAttribute : PropertyAttribute
    {
    }
}