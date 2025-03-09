// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.ServiceDefaults;

/// <summary>
///     Used to mark the assembly that contains the domain logic.
/// </summary>
/// <param name="typeMarker"></param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DomainAssemblyAttribute(Type typeMarker) : Attribute
{
    public Type DomainAssemblyTypeMarker { get; } = typeMarker;
}
