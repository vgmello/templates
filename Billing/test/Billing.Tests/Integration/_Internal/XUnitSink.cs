// Copyright (c) ABCDEG. All rights reserved.

using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Billing.Tests.Integration._Internal;

public class XUnitSink : ILogEventSink
{
    private readonly MessageTemplateTextFormatter _messageFormatter = new("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Exception}");

    private readonly List<string> _preInitializationMessages = [];

    public void Emit(LogEvent logEvent)
    {
        using var stringWriter = new StringWriter();
        _messageFormatter.Format(logEvent, stringWriter);
        var logMsg = stringWriter.ToString();

        var output = TestContext.Current.TestOutputHelper;

        if (output is null)
        {
            _preInitializationMessages.Add(logMsg);

            return;
        }

        if (_preInitializationMessages.Count > 0)
        {
            _preInitializationMessages.ForEach(output.WriteLine);
            _preInitializationMessages.Clear();
        }

        try
        {
            output.WriteLine(logMsg);
        }
        catch
        {
            // ignore any logging exception
        }
    }
}
