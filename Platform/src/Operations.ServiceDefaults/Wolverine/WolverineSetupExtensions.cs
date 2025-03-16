// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Runtime;

namespace Operations.ServiceDefaults.Wolverine;

public static class WolverineSetupExtensions
{
    public static IHostApplicationBuilder AddWolverine(this IHostApplicationBuilder builder,
        Action<WolverineOptions>? configure = null)
    {
        var wolverineRegistered = builder.Services.Any(s => s.ServiceType == typeof(IWolverineRuntime));

        if (wolverineRegistered)
            return builder;

        var handlerAssemblies = DomainAssemblyAttribute
            .GetDomainAssemblies()
            .Append(Extensions.EntryAssembly);

        return builder.UseWolverine(o =>
        {
            o.UseFluentValidation();

            foreach (var handlerAssembly in handlerAssemblies)
            {
                o.Discovery.IncludeAssembly(handlerAssembly);
            }

            configure?.Invoke(o);
        });
    }
}
