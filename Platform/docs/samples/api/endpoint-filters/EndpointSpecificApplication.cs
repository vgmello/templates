// Assuming definitions for app, CreateCashier, ValidationEndpointFilter, CreateCashierCommand,
// GetDetailedHealth, LocalhostEndpointFilter, LoggingEndpointFilter, ProcessPayment,
// RequireRoleEndpointFilter, GetData, RateLimitEndpointFilter.

var builder = WebApplication.CreateBuilder(args);
// Add services...
var app = builder.Build();


// Apply single filter
app.MapPost("/cashiers", CreateCashier)
    .AddEndpointFilter<ValidationEndpointFilter<CreateCashierCommand>>();

// Apply multiple filters
app.MapGet("/admin/health", GetDetailedHealth)
    .AddEndpointFilter<LocalhostEndpointFilter>()
    .AddEndpointFilter<LoggingEndpointFilter>();

// Apply filter with parameters
// Note: Requires RequireRoleEndpointFilter to have a constructor that accepts 'role'
app.MapPost("/payments", ProcessPayment)
    .AddEndpointFilter(new RequireRoleEndpointFilter("payment-processor"));

// Apply filter factory (if RateLimitEndpointFilter is registered as a service)
app.MapGet("/limited", GetData)
    .AddEndpointFilter<RateLimitEndpointFilter>();

app.Run();

// --- Placeholder definitions for context ---
// public static class WebApplicationExtensions { public static WebApplication Build(this WebApplicationBuilder builder) => builder.Build(); }
// public static class RouteHandlerBuilderExtensions { public static IEndpointConventionBuilder AddEndpointFilter<T>(this IEndpointConventionBuilder builder) where T : IEndpointFilter => builder; public static IEndpointConventionBuilder AddEndpointFilter(this IEndpointConventionBuilder builder, IEndpointFilter filter) => builder; }
// public interface IEndpointFilter { ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next); }
// public delegate ValueTask<object?> EndpointFilterDelegate(EndpointFilterInvocationContext context);
// public sealed class EndpointFilterInvocationContext { public HttpContext HttpContext { get; } public IList<object> Arguments { get; } public TGetArgument<T>(int index) => default; }

// public record CreateCashierCommand;
// public class ValidationEndpointFilter<T> : IEndpointFilter { public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext c, EndpointFilterDelegate n) => await n(c); }
// public static IResult CreateCashier(CreateCashierCommand cmd) => Results.Ok();
// public static IResult GetDetailedHealth() => Results.Ok();
// public class LocalhostEndpointFilter : IEndpointFilter { public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext c, EndpointFilterDelegate n) => await n(c); }
// public class LoggingEndpointFilter : IEndpointFilter { public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext c, EndpointFilterDelegate n) => await n(c); }
// public static IResult ProcessPayment() => Results.Ok();
// public class RequireRoleEndpointFilter : IEndpointFilter { public RequireRoleEndpointFilter(string role) { } public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext c, EndpointFilterDelegate n) => await n(c); }
// public static IResult GetData() => Results.Ok();
// public class RateLimitEndpointFilter : IEndpointFilter { public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext c, EndpointFilterDelegate n) => await n(c); }
// public class HttpContext { public IServiceProvider RequestServices { get; } }
// public class WebApplicationBuilder { public IServiceCollection Services { get; } = new ServiceCollection(); public static WebApplicationBuilder CreateBuilder(string[]? args = null) => new WebApplicationBuilder(); public WebApplication Build() => new WebApplication(Services.BuildServiceProvider()); }
// public class WebApplication : IEndpointRouteBuilder { private IServiceProvider _services; public WebApplication(IServiceProvider services) { _services = services; } public IApplicationBuilder CreateApplicationBuilder() => null; public IServiceProvider Services => _services; public IConfiguration Configuration => null; public IWebHostEnvironment Environment => null; public void Run() { } public void Run(string? url) { } public Task RunAsync(string? url = null) => Task.CompletedTask; public Task StartAsync(CancellationToken token = default) => Task.CompletedTask; public Task StopAsync(CancellationToken token = default) => Task.CompletedTask; public void Dispose() { } public ValueTask DisposeAsync() => ValueTask.CompletedTask; }
// public interface IServiceCollection : IList<ServiceDescriptor> { } public class ServiceCollection : List<ServiceDescriptor>, IServiceCollection { } public class ServiceDescriptor { }
// public interface IEndpointRouteBuilder { public IServiceProvider ServiceProvider { get; } public ICollection<EndpointDataSource> DataSources { get; } public IApplicationBuilder CreateApplicationBuilder(); }
// public class EndpointDataSource { } public interface IApplicationBuilder { } public interface IConfiguration { } public interface IWebHostEnvironment { } public class IResult { } public static class Results { public static IResult Ok() => null; } public class IEndpointConventionBuilder { }
// public static class EndpointRouteBuilderExtensions { public static IEndpointConventionBuilder MapGet(this IEndpointRouteBuilder e, string p, Delegate d) => null;  public static IEndpointConventionBuilder MapPost(this IEndpointRouteBuilder e, string p, Delegate d) => null; }
