// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.DependencyInjection;

namespace Housekeeping.BackOffice;

public static class DependencyInjection
{
    public static IServiceCollection AddBackOfficeServices(this IServiceCollection services)
    {
        // Add any additional BackOffice specific services here
        return services;
    }
}