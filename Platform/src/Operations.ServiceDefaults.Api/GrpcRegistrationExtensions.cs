// Copyright (c) ABCDEG. All rights reserved.

using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Operations.ServiceDefaults.Api;

[ExcludeFromCodeCoverage]
public static class GrpcRegistrationExtensions
{
    private const BindingFlags STATIC_METHODS = BindingFlags.Public | BindingFlags.Static;

    private static readonly MethodInfo GrpcMapServiceMethod = typeof(GrpcEndpointRouteBuilderExtensions)
        .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService), STATIC_METHODS)!;

    public static void MapGrpcServices(this IEndpointRouteBuilder routeBuilder) =>
        routeBuilder.MapGrpcServices(Assembly.GetEntryAssembly()!);

    public static void MapGrpcServices(this IEndpointRouteBuilder routeBuilder, Type assemblyMarker) =>
        routeBuilder.MapGrpcServices(assemblyMarker.Assembly);

    private static void MapGrpcServices(this IEndpointRouteBuilder routeBuilder, Assembly assembly)
    {
        var grpcServiceTypes = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false } && IsGrpcService(type));

        foreach (var grpcServiceType in grpcServiceTypes)
        {
            MapGrpcService(grpcServiceType, routeBuilder);
        }
    }

    private static void MapGrpcService(Type grpcServiceType, IEndpointRouteBuilder routeBuilder)
    {
        var typedGrpcMapServiceMethod = GrpcMapServiceMethod.MakeGenericMethod(grpcServiceType);
        typedGrpcMapServiceMethod.Invoke(null, new[] { routeBuilder });
    }

    private static bool IsGrpcService(Type type)
    {
        var grpcServiceAttribute = type.BaseType?.GetCustomAttribute<BindServiceMethodAttribute>();

        return grpcServiceAttribute is not null;
    }
}
