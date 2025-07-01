// Copyright (c) ABCDEG. All rights reserved.

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Housekeeping;

public static class DependencyInjection
{
    public static IServiceCollection AddHousekeepingWolverine(this IServiceCollection services, IConfiguration configuration,
        string? connectionString)
    {
        services.AddWolverine(opts =>
        {
            opts.Discovery.DisableConventionalDiscovery().IncludeAssembly(typeof(IHousekeepingAssembly).Assembly);
        });

        return services;
    }
}