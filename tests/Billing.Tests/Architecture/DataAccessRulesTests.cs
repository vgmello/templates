// Copyright (c) ABCDEG. All rights reserved.

using Billing.Core.Data;
using NetArchTest.Rules;

namespace Billing.Tests.Architecture;

#pragma warning disable CS8602

public class DataAccessRulesTests : ArchitectureTestBase
{
    [Fact]
    public void DataClasses_ShouldOnlyBeUsedByDomainClasses_ExceptCoreDataContext()
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
                .And().DoNotResideInNamespace($"{prefix}.Core.Data") // Allow Core data context to reference domain entities
                .And().DoNotHaveName("BillingDb") // Allow BillingDb to reference all domain data
                .Should().ResideInNamespace(prefix)
                .GetResult();

            var error =
                $"The following types depend on {prefix}.Data but don't reside in {prefix} namespace (Core data context and BillingDb excluded): " +
                $"{string.Join(", ", result.FailingTypeNames ?? [])}";

            result.IsSuccessful.ShouldBeTrue(error);
        }
    }

    [Fact]
    public void Entities_ShouldInheritFromDbEntity()
    {
        var result = GetBillingTypes()
            .That().ResideInNamespace("Billing")
            .And().ResideInNamespaceEndingWith(".Data.Entities")
            .And().AreClasses()
            .And().DoNotHaveName("DbEntity")
            .Should().Inherit(typeof(DbEntity))
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"All entities must inherit from DbEntity: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void BillingDb_ShouldOnlyBeUsedByCommandAndQueryHandlers()
    {
        var typesUsingBillingDb = GetBillingTypes()
            .That().HaveDependencyOn("BillingDb")
            .And().DoNotResideInNamespaceEndingWith(".Commands")
            .And().DoNotResideInNamespaceEndingWith(".Queries")
            .And().DoNotHaveName("BillingDb")
            .GetTypes()
            .Select(t => t.FullName)
            .ToList();

        typesUsingBillingDb.ShouldBeEmpty(
            $"BillingDb should only be used by command and query handlers: {string.Join(", ", typesUsingBillingDb)}");
    }
}
