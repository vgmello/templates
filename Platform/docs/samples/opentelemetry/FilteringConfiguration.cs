using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <FilteringRules>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddProcessor(new FilteringProcessor(activity =>
        !activity.DisplayName.Contains("/health")));
});
// </FilteringRules>

var app = builder.Build();
app.Run();

class FilteringProcessor : BaseProcessor<Activity>
{
    private readonly Func<Activity, bool> _filter;

    public FilteringProcessor(Func<Activity, bool> filter) => _filter = filter;

    public override void OnStart(Activity data)
    {
        if (!_filter(data)) data.IsAllDataRequested = false;
    }
}
