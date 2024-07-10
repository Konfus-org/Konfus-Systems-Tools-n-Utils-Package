using System;

/// <summary>
/// Marks a field or method to be injected by the dependency injector.
/// NOTE: for this to work there needs to be a DependencyInjector in the scene!
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
public sealed class InjectAttribute : Attribute
{
}

/// <summary>
/// Marks a field or method to be provided.
/// NOTE: for this to work there needs to be a DependencyInjector in the scene!
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
public sealed class ProvideAttribute : Attribute
{
}
