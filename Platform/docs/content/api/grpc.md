---
title: gRPC Service Registration
description: Learn how to automatically discover and register your gRPC services using reflection.
---

# gRPC service registration

The `GrpcRegistrationExtensions` class provides a convenient way to automatically discover and register all your gRPC services from a given assembly. This eliminates the need to manually map each service in your application's startup code.

## Automatic service discovery

The `MapGrpcServices` extension method scans an assembly for types that are concrete, non-abstract classes and are identified as gRPC services. A type is considered a gRPC service if its base class is annotated with the `[BindServiceMethod]` attribute, which is standard for generated gRPC service bases.

Once discovered, each service is automatically mapped using the underlying `MapGrpcService` method from `GrpcEndpointRouteBuilderExtensions`.

## Usage example

To register all gRPC services from the entry assembly, simply call `MapGrpcServices` on the `IEndpointRouteBuilder`.

[!code-csharp[](~/samples/api/grpc/MapGrpcServices.cs?highlight=3)]

Alternatively, you can specify an assembly by providing a marker type:

```csharp
// In your Program.cs or a startup class
app.MapGrpcServices(typeof(MyMarkerType));
```

This is particularly useful if your gRPC services reside in a different project or assembly.

## See also

-   [API Service Defaults](./service-defaults.md)
