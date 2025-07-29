// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Tests.Architecture;

#pragma warning disable CS8602

public class DependencyDirectionRulesTests : ArchitectureTestBase
{
    [Fact]
    public void ApiLayer_ShouldNotDependOnBackOffice()
    {
        var result = GetBillingTypes()
            .That().ResideInNamespace("Billing.Api")
            .Should().NotHaveDependencyOn("Billing.BackOffice")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"API layer should not depend on BackOffice: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void CoreDomain_ShouldNotDependOnApiOrBackOffice()
    {
        var result = GetBillingTypes()
            .That().ResideInNamespace("Billing")
            .And().DoNotResideInNamespace("Billing.Api")
            .And().DoNotResideInNamespace("Billing.BackOffice")
            .Should().NotHaveDependencyOnAny("Billing.Api", "Billing.BackOffice")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Core domain should not depend on API or BackOffice layers: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Contracts_ShouldNotDependOnImplementation()
    {
        var result = GetBillingTypes()
            .That().ResideInNamespaceEndingWith(".Contracts")
            .Should().NotHaveDependencyOnAny("Billing.Api", "Billing.BackOffice")
            .And().NotHaveDependencyOn("LinqToDB") // Contracts shouldn't depend on ORM
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Contracts should not depend on implementation details: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
