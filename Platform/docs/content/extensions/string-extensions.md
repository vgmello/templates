---
title: String extension methods
description: Explore string extension methods like ToSnakeCase and ToKebabCase for converting string casing, useful for consistent naming conventions.
---

# String extension methods for case conversion

The `StringExtensions` class provides a set of convenient extension methods for `string` objects, primarily focused on converting string casing. These methods are particularly useful for ensuring consistent naming conventions across your application, especially when interacting with systems that prefer specific casing styles, such as databases (snake_case) or web URLs (kebab-case).

## Understanding StringExtensions

This static class offers the following key methods:

*   **`ToSnakeCase()`**: Converts a string from PascalCase or camelCase to `snake_case` (e.g., `MyPropertyName` becomes `my_property_name`).
*   **`ToKebabCase()`**: Converts a string from PascalCase or camelCase to `kebab-case` (e.g., `MyPropertyName` becomes `my-property-name`).
*   **`ToLowerCaseWithSeparator(char separator)`**: A generalized internal method that performs the actual conversion to a lowercase string with a specified separator. `ToSnakeCase` and `ToKebabCase` use this method internally.

These methods handle various scenarios, including acronyms and digits, to produce accurate case conversions.

## Usage examples

Here's how you can use these extension methods in your code:

```csharp
using Operations.Extensions.Abstractions.Extensions;
using System;

public class CaseConversionExample
{
    public static void Main(string[] args)
    {
        string pascalCaseString = "ThisIsAPascalCaseString";
        string camelCaseString = "thisIsACamelCaseString";
        string acronymString = "APIEndpointURL";
        string mixedCaseString = "Product123Id";

        // Convert to snake_case
        Console.WriteLine($"PascalCase to SnakeCase: {pascalCaseString.ToSnakeCase()}");
        Console.WriteLine($"CamelCase to SnakeCase: {camelCaseString.ToSnakeCase()}");
        Console.WriteLine($"Acronym to SnakeCase: {acronymString.ToSnakeCase()}");
        Console.WriteLine($"MixedCase to SnakeCase: {mixedCaseString.ToSnakeCase()}");

        Console.WriteLine();

        // Convert to kebab-case
        Console.WriteLine($"PascalCase to KebabCase: {pascalCaseString.ToKebabCase()}");
        Console.WriteLine($"CamelCase to KebabCase: {camelCaseString.ToKebabCase()}");
        Console.WriteLine($"Acronym to KebabCase: {acronymString.ToKebabCase()}");
        Console.WriteLine($"MixedCase to KebabCase: {mixedCaseString.ToKebabCase()}");
    }
}
```

**Output of the example:**

```text
PascalCase to SnakeCase: this_is_a_pascal_case_string
CamelCase to SnakeCase: this_is_a_camel_case_string
Acronym to SnakeCase: api_endpoint_url
MixedCase to SnakeCase: product123_id

PascalCase to KebabCase: this-is-a-pascal-case-string
CamelCase to KebabCase: this-is-a-camel-case-string
Acronym to KebabCase: api-endpoint-url
MixedCase to KebabCase: product123-id
```

## See also

*   [Naming conventions](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines)
