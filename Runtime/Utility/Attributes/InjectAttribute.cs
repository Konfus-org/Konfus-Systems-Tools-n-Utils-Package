using System;

namespace Konfus.Utility.Attributes
{
    /// <summary>
    /// Marks a field or method to be injected by the dependency injector.
    /// NOTE: for this to work there needs to be a DependencyInjector in the scene!
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class InjectAttribute : Attribute
    {
    }
}