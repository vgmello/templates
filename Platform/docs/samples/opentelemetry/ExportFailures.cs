using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <ExportDebugging>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddProcessor(new ActivityExportExceptionHandler());
});
// </ExportDebugging>

var app = builder.Build();
app.Run();

class ActivityExportExceptionHandler : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        if (activity.Status == ActivityStatusCode.Error)
        {
            Console.Error.WriteLine($"Failed export: {activity.DisplayName}");
        }
    }
}
