using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <CommonIssues>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddProcessor(new ActivityExportExceptionHandler());
});
// </CommonIssues>

var app = builder.Build();
app.Run();

class ActivityExportExceptionHandler : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        if (activity.Status == ActivityStatusCode.Error)
        {
            Console.Error.WriteLine($"Export failed: {activity.DisplayName}");
        }
    }
}
