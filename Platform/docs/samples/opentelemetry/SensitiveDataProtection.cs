using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <DataProtection>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddProcessor(new SanitizeProcessor());
});
// </DataProtection>

var app = builder.Build();
app.Run();

class SanitizeProcessor : BaseProcessor<Activity>
{
    public override void OnStart(Activity activity)
    {
        activity.SetTag("user.password", "REDACTED");
    }
}
