// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Tests.Integration;

public static class ServiceCollectionExtensions
{
    public static void RemoveServices<TService>(this IServiceCollection services)
    {
        var registeredServices = services.Where(d => d.ServiceType == typeof(TService)).ToList();

        foreach (var registeredService in registeredServices)
            services.Remove(registeredService);
    }
}
