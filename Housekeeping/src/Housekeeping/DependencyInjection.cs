// Copyright (c) ABCDEG. All rights reserved.

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Operations.Extensions.Messaging;
using Wolverine;

namespace Housekeeping;

public static class DependencyInjection
{
    public static IServiceCollection AddHousekeepingWolverine(this IServiceCollection services, IConfiguration configuration,
        string? connectionString)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? throw new InvalidOperationException("Assembly name not found");

        services.AddWolverine(opts =>
        {
            opts.Discovery.DisableConventionalDiscovery().IncludeAssembly(typeof(IHousekeepingAssembly).Assembly);
            opts.AddPostgreSqlMessageingServices(connectionString, assemblyName);
        });

        return services;
    }
}