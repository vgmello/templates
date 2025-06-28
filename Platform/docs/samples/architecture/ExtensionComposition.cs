using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.Messaging.Wolverine;

// <ExtensionComposition>
// Rather than large configuration classes, the Platform uses focused 
// extension methods that can be composed together

var builder = WebApplication.CreateBuilder(args);

// Core infrastructure extensions
builder.AddServiceDefaults();          // Logging, health checks, telemetry
builder.AddApiServiceDefaults();       // API-specific features

// Feature-specific extensions that compose together
builder.AddWolverineMessaging();       // Message bus and handlers
builder.AddDatabaseIntegration();      // Database connections and migrations  
builder.AddExternalServices();         // HTTP clients for external APIs

// Each extension is focused and can be added/removed independently
// Extensions are designed to work together without conflicts
// </ExtensionComposition>

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();

// <CustomExtensions>
// You can create your own focused extension methods following the same pattern

public static class DatabaseExtensions
{
    public static IHostApplicationBuilder AddDatabaseIntegration(this IHostApplicationBuilder builder)
    {
        // Register database-related services
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        
        // Add database health checks
        builder.Services.AddHealthChecks()
            .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);
        
        // Add migration services
        builder.Services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        
        return builder;
    }
}

public static class ExternalServiceExtensions
{
    public static IHostApplicationBuilder AddExternalServices(this IHostApplicationBuilder builder)
    {
        // Configure HTTP clients for external services
        builder.Services.AddHttpClient<IPaymentService, PaymentService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["ExternalServices:Payment:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        builder.Services.AddHttpClient<INotificationService, NotificationService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["ExternalServices:Notifications:BaseUrl"]!);
        });
        
        // Add health checks for external services
        builder.Services.AddHealthChecks()
            .AddUrlGroup(new Uri(builder.Configuration["ExternalServices:Payment:HealthUrl"]!), "payment_service")
            .AddUrlGroup(new Uri(builder.Configuration["ExternalServices:Notifications:HealthUrl"]!), "notification_service");
        
        return builder;
    }
}
// </CustomExtensions>

// <ServiceInterfaces>
// Example service interfaces
public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task SendNotificationAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}

public interface IDatabaseMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken = default);
}

// Example implementations would be here...
public class PaymentService : IPaymentService
{
    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class NotificationService : INotificationService
{
    public Task SendNotificationAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class DatabaseMigrator : IDatabaseMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class ApplicationDbContext : DbContext
{
}

public record PaymentRequest;
public record PaymentResult;
public record NotificationRequest;
// </ServiceInterfaces>