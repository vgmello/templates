// Copyright (c) ABCDEG. All rights reserved.

using FluentValidation;
using JasperFx;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults.Logging;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Operations.ServiceDefaults.OpenTelemetry;
using Serilog;
using System.Reflection;

namespace Operations.ServiceDefaults;

public static class Extensions
{
    private static Assembly? _entryAssembly;

    public static Assembly EntryAssembly
    {
        get => _entryAssembly ??= GetEntryAssembly();
        set => _entryAssembly = value;
    }

    public static IHostApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.WebHost.UseKestrelHttpsConfiguration();

        builder.AddLogging();
        builder.AddOpenTelemetry();
        builder.AddWolverine();
        builder.AddValidators();

        builder.Services.AddHealthChecks();
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();
        });

        return builder;
    }

    public static void AddValidators(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(EntryAssembly);

        foreach (var assembly in DomainAssemblyAttribute.GetDomainAssemblies())
            builder.Services.AddValidatorsFromAssembly(assembly);
    }

    private static Assembly GetEntryAssembly()
    {
        return Assembly.GetEntryAssembly() ??
               throw new InvalidOperationException(
                   "Unable to identify entry assembly. Please provide an assembly via the Extensions.AssemblyMarker property.");
    }

    public static async Task RunAsync(this WebApplication app, string[] args)
    {
        app.UseInitializationLogger();
        
        try
        {
            if (args.Length > 0 && WolverineCommands.Contains(args[0]))
            {
                await app.RunJasperFxCommands(args);
            }

            await app.RunAsync();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static readonly HashSet<string> WolverineCommands =
    [
        "check-env",
        "codegen",
        "db-apply",
        "db-assert",
        "db-dump",
        "db-patch",
        "describe",
        "help",
        "resources",
        "storage"
    ];
}
