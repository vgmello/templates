// Copyright (c) ABCDEG. All rights reserved.

using NetArchTest.Rules;

namespace Billing.Tests.Architecture;

#pragma warning disable CS8602

/// <summary>
///     Base class for all architecture tests providing common functionality.
/// </summary>
public abstract class ArchitectureTestBase
{
    /// <summary>
    ///     Gets all types from the Billing assemblies for architecture testing.
    /// </summary>
    protected static Types GetBillingTypes() => Types
        .InAssemblies([typeof(IBillingAssembly).Assembly, typeof(Api.DependencyInjection).Assembly]);
}
