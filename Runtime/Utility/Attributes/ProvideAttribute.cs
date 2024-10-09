using System;

namespace Konfus.Utility.Attributes
{
    /// <summary>
    /// Marks a field or method to be provided.
    /// NOTE: for this to work there needs to be a DependencyInjector in the scene!
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class ProvideAttribute : Attribute
    {
    }
}
