// Copyright (c) ABCDEG. All rights reserved.

using System.Collections.Immutable;
using System.Reflection;

namespace Operations.ServiceDefaults;

/// <summary>
///     Used to mark the assembly that contains the domain logic.
/// </summary>
/// <param name="typeMarkers"></param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DomainAssemblyAttribute(params Type[] typeMarkers) : Attribute
{
    private Type[] DomainAssemblyTypeMarkers => typeMarkers;

    internal static IReadOnlyList<Assembly> GetDomainAssemblies(Assembly? applicationAssembly = null)
    {
        var targetAssembly = applicationAssembly ?? Extensions.EntryAssembly;

        return targetAssembly.GetCustomAttribute<DomainAssemblyAttribute>()?
            .DomainAssemblyTypeMarkers
            .Select(t => t.Assembly)
            .ToImmutableList() ?? [];
    }
}
