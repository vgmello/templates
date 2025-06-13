using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using Wolverine;
using Operations.ServiceDefaults.Messaging.Middlewares;
using System;
using System.Diagnostics;
using System.Collections.Generic; // Required for KeyValuePair
using Operations.ServiceDefaults.Messaging.Middlewares; // Ensure this points to the actual middleware

// Define test messages in a namespace to test namespace extraction
namespace Operations.ServiceDefaults.UnitTests.Messaging.Middlewares.TestMessages
{
    public class TestCommand
    {
        public string Data { get; set; }
    }

    public class AnotherTestCommand // No namespace apart from the file's
    {
        public string Info { get; set; }
    }
}

namespace Operations.ServiceDefaults.UnitTests.Messaging.Middlewares
{
    public class RequestPerformanceMiddlewareTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<Meter> _mockMeter;
        private readonly Mock<Counter<long>> _mockCounter;
        private readonly Mock<Histogram<double>> _mockHistogram;

        // Envelope needs to be created per test or with specific message types
        // private Envelope _testEnvelope;
        // private TestMessages.TestCommand _testMessage;

        public RequestPerformanceMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockMeter = new Mock<Meter>("Test.App.Commands");
            _mockCounter = new Mock<Counter<long>>();
            _mockHistogram = new Mock<Histogram<double>>();

            // Setup Meter to return mocked instruments. This will be general setup.
            // Specific name verification will be in each test.
            _mockMeter.Setup(m => m.CreateCounter<long>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                      .Returns(_mockCounter.Object);
            _mockMeter.Setup(m => m.CreateHistogram<double>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                      .Returns(_mockHistogram.Object);
        }

        [Theory]
        [InlineData("Accounting.Cashier.Data.CreateCashierCommand", "accounting.cashier", "createcashiercommand")]
        [InlineData("Orders.ProcessOrderCommand", "orders", "processordercommand")]
        [InlineData("MyApp.Core.Users.UpdateUser", "myapp.core", "updateuser")] // Example with 3 parts
        [InlineData("SimpleCommand", "", "simplecommand")] // No namespace
        [InlineData("InternalMessages.NotificationEvent", "internalmessages", "notificationevent")]
        [InlineData("MyCompany.MyProduct.Users.Commands.RegisterUserCommand", "mycompany.myproduct", "registerusercommand")]
        [InlineData("SingleNameSpace.MyCommand", "singlenamespace", "mycommand")]
        [InlineData("System.Guid", "system", "guid")] // If GetMessageTypeName returns System.Guid for some reason
        [InlineData("a.b.c.d.e.ClassName", "a.b", "classname")] // More than 2 namespace parts
        [InlineData("", "unknown", "unknown_type")] // Empty string
        [InlineData(null, "unknown", "unknown_type")] // Null string
        [InlineData("123e4567-e89b-12d3-a456-426614174000", "system", "message_id")] // GUID string
        public void ExtractMetricParts_ReturnsCorrectParts(string fullTypeName, string expectedPrefix, string expectedCommandName)
        {
            // Act
            var parts = RequestPerformanceMiddleware.ExtractMetricParts(fullTypeName);

            // Assert
            Assert.Equal(expectedPrefix, parts.Prefix);
            Assert.Equal(expectedCommandName, parts.CommandName);
        }

        private Envelope CreateEnvelopeForTest(object message, string fullTypeNameOverride = null)
        {
            var envelope = new Envelope(message);
            if (message != null)
            {
                envelope.MessageType = fullTypeNameOverride ?? message.GetType().FullName;
            }
            else if(fullTypeNameOverride != null)
            {
                envelope.MessageType = fullTypeNameOverride;
            }
            return envelope;
        }

        [Fact]
        public void Before_CreatesAndIncrementsCounter_WithNamespace()
        {
            // Arrange
            var testMessage = new TestMessages.TestCommand { Data = "test" };
            var envelope = CreateEnvelopeForTest(testMessage);
            // Expected: operations.servicedefaults.unittest.messaging.middlewares.testmessages.testcommand.count
            // based on file path. Let's assume ExtractMetricParts works as tested above.
            // For TestMessages.TestCommand, namespace is Operations.ServiceDefaults.UnitTests.Messaging.Middlewares.TestMessages
            // Prefix becomes: operations.servicedefaults
            // Command name: testcommand
            var expectedPrefix = "operations.servicedefaults"; // From namespace
            var expectedCommandName = "testcommand";
            var expectedMetricName = $"{expectedPrefix}.{expectedCommandName}.count";

            // Act
            RequestPerformanceMiddleware.Before(_mockLogger.Object, envelope, _mockMeter.Object);

            // Assert
            _mockMeter.Verify(m => m.CreateCounter<long>(
                expectedMetricName,
                "invocations",
                "Number of times the command has been invoked."), Times.Once);
            _mockCounter.Verify(c => c.Add(1, It.IsAny<ReadOnlySpan<KeyValuePair<string, object?>>>()), Times.Once);
        }

        [Fact]
        public void Finally_CreatesAndRecordsHistogram_WithNamespace()
        {
            // Arrange
            var testMessage = new TestMessages.TestCommand { Data = "test" };
            var envelope = CreateEnvelopeForTest(testMessage);
            var startedTime = Stopwatch.GetTimestamp();
            Stopwatch.GetElapsedTime(startedTime); // Simulate time passing

            var expectedPrefix = "operations.servicedefaults";
            var expectedCommandName = "testcommand";
            var expectedMetricName = $"{expectedPrefix}.{expectedCommandName}.duration";

            // Act
            RequestPerformanceMiddleware.Finally(startedTime, _mockLogger.Object, envelope, _mockMeter.Object);

            // Assert
            _mockMeter.Verify(m => m.CreateHistogram<double>(
                expectedMetricName,
                "ms",
                "Execution duration of the command."), Times.Once);
            _mockHistogram.Verify(h => h.Record(It.IsAny<double>(), It.IsAny<ReadOnlySpan<KeyValuePair<string, object?>>>()), Times.Once);
        }

        [Fact]
        public void Before_UsesCorrectMetricName_WhenMessageTypeIsGuidString()
        {
            // Arrange
            var guidString = Guid.NewGuid().ToString();
            var envelopeWithNoMessage = CreateEnvelopeForTest(null, guidString); // MessageType is the GUID string

            var expectedMetricName = "system.message_id.count"; // As per ExtractMetricParts logic

            // Act
            RequestPerformanceMiddleware.Before(_mockLogger.Object, envelopeWithNoMessage, _mockMeter.Object);

            // Assert
            _mockMeter.Verify(m => m.CreateCounter<long>(
                expectedMetricName,
                "invocations",
                "Number of times the command has been invoked."), Times.Once);
            _mockCounter.Verify(c => c.Add(1, It.IsAny<ReadOnlySpan<KeyValuePair<string, object?>>>()), Times.Once);
        }

        [Fact]
        public void OnException_CreatesAndIncrementsExceptionCounter_AndLogsError()
        {
            // Arrange
            // Using a type from the TestMessages namespace for consistency
            var failingMessage = new TestMessages.AnotherTestCommand { Info = "failure expected" };
            var envelope = CreateEnvelopeForTest(failingMessage);
            var exception = new InvalidOperationException("Test failure");

            // Expected prefix from Operations.ServiceDefaults.UnitTests.Messaging.Middlewares.TestMessages
            var expectedPrefix = "operations.servicedefaults";
            var expectedCommandName = "anothertestcommand";
            var expectedMetricName = $"{expectedPrefix}.{expectedCommandName}.exceptions";

            // Act
            RequestPerformanceMiddleware.OnException(0L, _mockLogger.Object, envelope, exception, _mockMeter.Object);

            // Assert
            _mockMeter.Verify(m => m.CreateCounter<long>(
                expectedMetricName,
                "exceptions",
                "Number of times the command processing resulted in an exception."), Times.Once);
            _mockCounter.Verify(c => c.Add(1, It.IsAny<ReadOnlySpan<KeyValuePair<string, object?>>>()), Times.Once);

            // Verify logging (basic check that some error was logged)
            // This relies on the fact that LogRequestFailed is the only Error level log in OnException.
            // A more specific check would require capturing log messages or a more advanced mock.
            _mockLogger.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(), // EventId for LogRequestFailed is 3
                It.IsAny<It.IsAnyType>(), // Formatted message
                exception, // The exception instance
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), // Formatter function
                Times.Once);
        }
    }
}
