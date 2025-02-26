// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.ServiceDefaults;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DomainAssemblyAttribute(Type typeMarker) : Attribute
{
    public Type DomainAssemblyTypeMarker { get; } = typeMarker;
}
