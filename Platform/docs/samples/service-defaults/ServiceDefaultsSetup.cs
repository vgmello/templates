using Operations.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// #region ServiceDefaultsConfiguration
// Add all service defaults with one call
builder.AddServiceDefaults();
// #endregion

// Add your application-specific services
builder.Services.AddScoped<IMyService, MyService>();

var app = builder.Build();

// #region DefaultEndpoints
// Configure default endpoints
app.MapDefaultEndpoints();
// #endregion

// #region EnhancedStartup
// Use enhanced startup with proper error handling
await app.RunAsync(args);
// #endregion

public interface IMyService
{
    Task DoSomethingAsync();
}

public class MyService : IMyService
{
    public Task DoSomethingAsync() => Task.CompletedTask;
}