using Xunit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Operations.ServiceDefaults.Messaging.Middlewares; // For RequestPerformanceMiddleware if needed directly, though it's applied by Wolverine
using Microsoft.Extensions.Logging;
using System; // Required for Exception

namespace Operations.ServiceDefaults.IntegrationTests.Messaging
{
    // Define commands and handlers within the current namespace for predictable metric names
    public class SampleIntegrationCommand
    {
        public string Data { get; set; }
    }

    public class SampleIntegrationCommandHandler
    {
        private readonly ILogger<SampleIntegrationCommandHandler> _logger;
        public SampleIntegrationCommandHandler(ILogger<SampleIntegrationCommandHandler> logger) => _logger = logger;

        public async Task Handle(SampleIntegrationCommand command)
        {
            _logger.LogInformation("Handling SampleIntegrationCommand: {Data}", command.Data);
            await Task.Delay(50); // Simulate work
        }
    }

    public class FailingIntegrationCommand
    {
        public string Data { get; set; }
    }

    public class FailingIntegrationCommandHandler
    {
        private readonly ILogger<FailingIntegrationCommandHandler> _logger;
        public FailingIntegrationCommandHandler(ILogger<FailingIntegrationCommandHandler> logger) => _logger = logger;

        public Task Handle(FailingIntegrationCommand command)
        {
            _logger.LogInformation("Handling FailingIntegrationCommand: {Data}, which will throw.", command.Data);
            throw new InvalidOperationException("Simulated failure for FailingIntegrationCommand.");
        }
    }

    public class RequestPerformanceMiddlewareIntegrationTests : IAsyncLifetime
    {
        private IHost _host;
        private List<Metric> _exportedMetrics;
        private Meter _commandMeter;
        private const string CommandMeterName = "TestApp.Integration.Commands";
        private const string ExpectedNamespacePrefix = "operations.servicedefaults"; // Based on this file's namespace

        public async Task InitializeAsync()
        {
            _exportedMetrics = new List<Metric>();
            _commandMeter = new Meter(CommandMeterName);

            _host = await Host.CreateDefaultBuilder()
                .ConfigureLogging(logging => logging.ClearProviders().AddDebug())
                .UseWolverine(opts =>
                {
                    // Ensure Wolverine scans this assembly for handlers
                    opts.ApplicationAssembly = typeof(RequestPerformanceMiddlewareIntegrationTests).Assembly;

                    // The middleware is applied by Wolverine conventions if discoverable.
                    // We register the Meter instance it needs.
                    opts.Services.AddSingleton(_commandMeter);

                    opts.Services.AddOpenTelemetry().WithMetrics(builder =>
                    {
                        builder.AddMeter(CommandMeterName);
                        builder.AddInMemoryExporter(_exportedMetrics);
                    });
                })
                .StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _host.StopAsync();
            _host.Dispose();
            _commandMeter.Dispose();
        }

        private void AssertMetricValue(string metricName, MetricType expectedType, long expectedValue, string pointType = "sum")
        {
            var metric = _exportedMetrics.FirstOrDefault(m => m.Name == metricName);
            Assert.NotNull(metric);
            Assert.Equal(expectedType, metric.MetricType);

            long actualValue = 0;
            bool pointFound = false;
            foreach (var mp in metric.GetMetricPoints())
            {
                pointFound = true;
                if (pointType == "sum") actualValue += mp.GetSumLong();
                else if (pointType == "count") actualValue += mp.GetHistogramCount(); // For histogram total count
            }
            Assert.True(pointFound, $"No metric points found for {metricName}.");
            Assert.Equal(expectedValue, actualValue);
        }

        private void AssertHistogramObservation(string metricName)
        {
            var metric = _exportedMetrics.FirstOrDefault(m => m.Name == metricName);
            Assert.NotNull(metric);
            Assert.Equal(MetricType.Histogram, metric.MetricType);

            bool pointFound = false;
            foreach (var mp in metric.GetMetricPoints())
            {
                pointFound = true;
                Assert.True(mp.GetHistogramCount() >= 1, $"Histogram {metricName} should have at least one observation.");
            }
            Assert.True(pointFound, $"No metric points found for {metricName}.");
        }


        [Fact]
        public async Task Middleware_ShouldRecordCountAndDurationMetrics_ForSuccessfulCommand()
        {
            // Arrange
            var messageBus = _host.Services.GetRequiredService<IMessageBus>();
            var command = new SampleIntegrationCommand { Data = "Integration Test Data" };

            var commandName = nameof(SampleIntegrationCommand).ToLowerInvariant();
            var expectedCounterName = $"{ExpectedNamespacePrefix}.{commandName}.count";
            var expectedHistogramName = $"{ExpectedNamespacePrefix}.{commandName}.duration";

            // Act
            await messageBus.SendAsync(command);
            await Task.Delay(200); // Allow time for metrics export

            // Assert
            Assert.NotEmpty(_exportedMetrics);
            AssertMetricValue(expectedCounterName, MetricType.LongSum, 1);
            AssertHistogramObservation(expectedHistogramName);
        }

        [Fact]
        public async Task Middleware_ShouldRecordExceptionMetric_ForFailedCommand()
        {
            // Arrange
            var messageBus = _host.Services.GetRequiredService<IMessageBus>();
            var command = new FailingIntegrationCommand { Data = "Failure Test Data" };

            var commandName = nameof(FailingIntegrationCommand).ToLowerInvariant();
            var expectedCounterName = $"{ExpectedNamespacePrefix}.{commandName}.count";
            var expectedHistogramName = $"{ExpectedNamespacePrefix}.{commandName}.duration";
            var expectedExceptionCounterName = $"{ExpectedNamespacePrefix}.{commandName}.exceptions";

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => messageBus.SendAsync(command));
            await Task.Delay(200); // Allow time for metrics export

            // Assert
            Assert.NotEmpty(_exportedMetrics);

            // Check standard count metric (Before phase should have run)
            AssertMetricValue(expectedCounterName, MetricType.LongSum, 1);

            // Check exception count metric
            AssertMetricValue(expectedExceptionCounterName, MetricType.LongSum, 1);

            // Check duration metric (Finally phase should also run)
            AssertHistogramObservation(expectedHistogramName);
        }
    }
}
