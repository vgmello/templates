using System.Reflection;
using Operations.ServiceDefaults;

public class ApplicationStartup
{
    public void ConfigureServices()
    {
        // Get domain assemblies from the entry assembly
        var domainAssemblies = DomainAssemblyAttribute.GetDomainAssemblies(Assembly.GetEntryAssembly());

        foreach (var assembly in domainAssemblies)
        {
            Console.WriteLine($"Found domain assembly: {assembly.FullName}");
            // You can now use reflection to find types, register services, etc.
        }
    }
}
