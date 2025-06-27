using Microsoft.Extensions.DependencyInjection;

public static class ExtensionComposition
{
    public static IServiceCollection AddFeatureA(this IServiceCollection services)
    {
        // Add services for Feature A
        return services;
    }

    public static IServiceCollection AddFeatureB(this IServiceCollection services)
    {
        // Add services for Feature B
        return services;
    }

    public static void ConfigureFeatures(IServiceCollection services)
    {
        services.AddFeatureA()
                .AddFeatureB();
    }
}