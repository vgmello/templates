using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace Operations.ServiceDefaults;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseEntryAssembly<T>(this IHostBuilder builder)
    {
        Extensions.EntryAssembly = typeof(T).Assembly;
        return builder;
    }

    public static IWebHostBuilder UseEntryAssembly<T>(this IWebHostBuilder builder)
    {
        Extensions.EntryAssembly = typeof(T).Assembly;
        return builder;
    }
}
