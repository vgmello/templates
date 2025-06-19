// Copyright (c) ABCDEG. All rights reserved.

using NetArchTest.Rules;

namespace Billing.Tests.Architecture;

#pragma warning disable CS8602

public class ArchTests
{
    [Fact]
    public void DataClasses_ShouldOnlyBeUsedByDomainClasses()
    {
        var assemblies = new[] { typeof(IBillingAssembly).Assembly, typeof(Api.DependencyInjection).Assembly };

        var dataNamespaces = assemblies
            .SelectMany(a => a.GetTypes())
            .Select(t => t.Namespace)
            .Where(ns => ns is not null && (ns.Contains(".Data.") || ns.EndsWith(".Data")))
            .Select(ns => ns![..ns.IndexOf(".Data", StringComparison.Ordinal)])
            .Distinct()
            .ToList();

        foreach (var prefix in dataNamespaces.Where(ns => !ns.EndsWith(".Core")))
        {
            var result = Types
                .InAssemblies(assemblies)
                .That().HaveDependencyOn($"{prefix}.Data")
                .Should().ResideInNamespace(prefix)
                .GetResult();

            var error = $"The following types depend on {prefix}.Data but don't reside in {prefix} namespace: " +
                        $"{string.Join(", ", result.FailingTypeNames ?? [])}";

            result.IsSuccessful.ShouldBeTrue(error);
        }
    }
}
