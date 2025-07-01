// Copyright (c) ABCDEG. All rights reserved.

using System.Diagnostics;
using CommandLine;
using PeterKottas.DotNetCore.WindowsService;

namespace Housekeeping.Database;

internal class Program
{
    internal class Options
    {
        [Option('a', "action", Default = "start", HelpText = "Service action: run, start, stop, or custom")]
        public string Action { get; set; } = "start";
    }

    private static void Main(string[] args)
    {
        ServiceRunner<Service>.Run(config =>
        {
            config.SetName("HousekeepingDatabaseService");
            config.SetDisplayName("Housekeeping Database Service");
            config.SetDescription("Database migration service for Housekeeping");

            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments, controller) => new Service());

                    serviceConfig.OnStart((service, extraArguments) =>
                    {
                        Console.WriteLine("Service {0} started", config.GetDefaultName());
                        service.Start();
                    });

                    serviceConfig.OnStop(service =>
                    {
                        Console.WriteLine("Service {0} stopped", config.GetDefaultName());
                        service.Stop();
                    });

                    serviceConfig.OnError(e =>
                    {
                        Console.WriteLine("Service {0} encountered an error: {1}", config.GetDefaultName(), e.Message);
                    });
                });

                config.Action = options.Action switch
                {
                    "run" => ActionEnum.Run,
                    "install" => ActionEnum.Install,
                    "uninstall" => ActionEnum.Uninstall,
                    "start" => ActionEnum.Start,
                    "stop" => ActionEnum.Stop,
                    _ => ActionEnum.RunInteractive
                };
            });
        });
    }
}

internal class Service
{
    private readonly Dictionary<string, string> _updates = new()
    {
        { "Setup", "liquibase.setup.properties" },
        { "Housekeeping", "liquibase.properties" },
        { "ServiceBus", "liquibase.servicebus.properties" }
    };

    public void Start()
    {
        RunLiquibase();
    }

    public void Stop()
    {
    }

    private void RunLiquibase()
    {
        // TODO: Replace with Docker.DotNet to execute liquibase
        var dockerPath = OperatingSystem.IsWindows() ? "docker.exe" : "docker";

        foreach (var update in _updates)
        {
            Console.WriteLine($"Running {update.Key} update using {update.Value}");

            var arguments = $"run --rm -v {Directory.GetCurrentDirectory()}:/liquibase/changelog liquibase/liquibase --defaultsFile=/liquibase/changelog/{update.Value} update";

            var processStartInfo = new ProcessStartInfo(dockerPath, arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                Console.WriteLine($"Failed to start process for {update.Key}");
                continue;
            }

            process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(output))
                Console.WriteLine(output);

            if (!string.IsNullOrEmpty(error))
                Console.WriteLine($"Error: {error}");

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Liquibase update failed for {update.Key} with exit code {process.ExitCode}");
            }
            else
            {
                Console.WriteLine($"Successfully completed {update.Key} update");
            }
        }
    }
}