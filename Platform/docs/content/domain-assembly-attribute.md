---
title: Domain Assembly Attribute
description: Learn how to mark assemblies containing your domain logic using the DomainAssemblyAttribute.
---

# Domain Assembly Attribute

Use `DomainAssemblyAttribute` to mark assemblies that contain your domain logic. This helps you discover and work with domain-specific types across projects.

## Purpose

You'll use this attribute to help frameworks find your domain assemblies at runtime. It tells them exactly where your core business logic lives. This is especially useful when frameworks need to scan for specific types or configurations.

## Usage example

Apply `DomainAssemblyAttribute` at the assembly level. You can add it to `AssemblyInfo.cs` or your project's `Program.cs` file. You'll need to provide type markers from the domain assembly to identify it.

Let's say you have a domain assembly named `YourApp.Domain` with a type `YourDomainType`. Here's how you'd mark it:

```csharp
// In YourApp.Domain/AssemblyInfo.cs or a similar file
using Operations.ServiceDefaults;
using YourApp.Domain.Entities; // Assuming YourDomainType is in Entities namespace

[assembly: DomainAssembly(typeof(YourDomainType))]
```

### Retrieving domain assemblies

Use the `GetDomainAssemblies` helper method to retrieve marked assemblies. You can call this from your application's entry point or anywhere you need to discover domain assemblies.

```csharp
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
```

## See also

- [Extensions](extensions/overview.md)
