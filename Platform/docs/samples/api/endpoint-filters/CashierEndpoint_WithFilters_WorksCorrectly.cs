// CashierEndpoint_WithFilters_WorksCorrectly.cs
using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

public class CashierEndpoint_WithFilters_WorksCorrectly
{
    [Fact]
    public async Task CashierEndpoint_WithFilters_WorksCorrectly()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/cashiers", new CreateCashierCommand
        {
            Name = "Test Cashier",
            Email = "test@example.com",
            Currencies = new[] { "USD" }
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddScoped<LoggingEndpointFilter>();
        builder.Services.AddScoped<ValidationEndpointFilter<CreateCashierCommand>>();
        builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
        builder.Services.AddSingleton<IValidator<CreateCashierCommand>, CreateCashierCommandValidator>();

        var app = builder.Build();

        app.MapPost("/cashiers", (CreateCashierCommand command) => Results.Created($"/cashiers/{Guid.NewGuid()}", command))
            .AddEndpointFilter<LoggingEndpointFilter>()
            .AddEndpointFilter<ValidationEndpointFilter<CreateCashierCommand>>()
            .AddEndpointFilter<RateLimitEndpointFilter>();

        app.Run();
    }
}

public record CreateCashierCommand
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string[] Currencies { get; set; }
}

public class CreateCashierCommandValidator : AbstractValidator<CreateCashierCommand>
{
    public CreateCashierCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Currencies).NotEmpty();
    }
}
