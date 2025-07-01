// Copyright (c) ABCDEG. All rights reserved.

using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace Housekeeping.Tests.Architecture;

public class ArchTests
{
    private static readonly Assembly DomainAssembly = typeof(IHousekeepingAssembly).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Housekeeping.Api.Rooms.RoomsController).Assembly;

    [Fact]
    public void Domain_Should_Not_Have_Dependency_On_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .And()
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Commands_Should_Be_Immutable()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(ICommand<>))
            .Should()
            .BeRecord()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Queries_Should_Be_Immutable()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .BeRecord()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Validators_Should_Follow_Naming_Convention()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}