---
title: Domain Assembly Attribute
description: Learn how to mark assemblies containing your domain logic using the DomainAssemblyAttribute.
---

# Domain Assembly Attribute

The `DomainAssemblyAttribute` is a powerful tool that allows you to explicitly mark assemblies containing your application's domain logic. This helps in scenarios where you need to discover and work with domain-specific types or configurations across different projects.

## Purpose

This attribute is primarily used to facilitate the discovery of domain assemblies at runtime. By applying `DomainAssemblyAttribute` to an assembly, you provide a clear indicator of where your core business logic resides. This is especially useful for frameworks or libraries that need to scan for specific types or configurations within your domain.

## Usage example

To use the `DomainAssemblyAttribute`, you apply it at the assembly level, typically in a file like `AssemblyInfo.cs` or directly in your project's `Program.cs` if you're using top-level statements. You provide one or more type markers from the domain assembly. These markers are used to identify the assembly.

Consider you have a domain assembly named `YourApp.Domain` and a type `YourDomainType` within it. You would mark the assembly like this:

[!code-csharp[](~/docs/samples/domain-assembly-attribute/DomainAssemblyAttributeUsage.cs)]

### Retrieving domain assemblies

The `DomainAssemblyAttribute` provides a static helper method, `GetDomainAssemblies`, to retrieve the marked assemblies. This method can be called from your application's entry point or any other part of your application that needs to discover domain assemblies.

[!code-csharp[](~/docs/samples/domain-assembly-attribute/GetDomainAssemblies.cs)]

## See also

- [Extensions](extensions.md)
