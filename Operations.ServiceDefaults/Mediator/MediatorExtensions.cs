// Copyright (c) ABCDEG. All rights reserved.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Operations.ServiceDefaults.Mediator;

public static class MediatorExtensions
{
    public static Assembly? AssemblyMarker { get; set; }

    public static IHostApplicationBuilder AddMediator(this IHostApplicationBuilder builder, Action<MediatRServiceConfiguration>? configuration = null)
    {
        var mediatorRegistered = builder.Services.Any(s => s.ServiceType == typeof(MediatR.IMediator));

        if (mediatorRegistered)
            return builder;

        var assemblyMarker = AssemblyMarker ?? Assembly.GetEntryAssembly();

        if (assemblyMarker is null)
        {
            throw new InvalidOperationException("Unable to identify the assembly to scan for Mediator handlers. " +
                                                "Please provide an assembly via the MediatorExtensions.AssemblyMarker property.");
        }

        var domainAssembly = assemblyMarker.GetCustomAttribute<DomainAssemblyAttribute>()?.DomainAssemblyTypeMarker.Assembly;

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assemblyMarker);

            if (domainAssembly is not null)
                cfg.RegisterServicesFromAssembly(domainAssembly);

            configuration?.Invoke(cfg);
        });

        builder.AddCommandAndQueryServices();

        return builder;
    }

    public static IHostApplicationBuilder AddCommandAndQueryServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<ICommandServices, CommandServices>();
        return builder;
    }
}
