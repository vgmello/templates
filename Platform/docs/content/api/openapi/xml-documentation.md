# XML Documentation Integration

This guide covers how XML documentation comments are integrated into OpenAPI specifications in the Operations platform.

## Overview

The Operations platform automatically processes XML documentation comments and incorporates them into the generated OpenAPI specification, providing rich API documentation without additional configuration.

## XML Documentation Comments

### Controller Documentation

```csharp
/// <summary>
/// Manages cashier operations including creation, updates, and retrieval
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Cashiers")]
public class CashiersController : ControllerBase
{
    // Controller implementation
}
```

### Action Documentation

```csharp
/// <summary>
/// Creates a new cashier in the system
/// </summary>
/// <param name="command">The cashier creation command containing name, email, and supported currencies</param>
/// <param name="cancellationToken">Cancellation token to cancel the operation</param>
/// <returns>The created cashier with assigned ID and creation timestamp</returns>
/// <response code="201">Cashier created successfully</response>
/// <response code="400">Invalid request data or validation failures</response>
/// <response code="409">Cashier with the same email already exists</response>
/// <remarks>
/// Sample request:
/// 
///     POST /api/cashiers
///     {
///         "name": "John Doe",
///         "email": "john.doe@example.com",
///         "currencies": ["USD", "EUR"]
///     }
/// 
/// </remarks>
[HttpPost]
[ProducesResponseType<CashierResponse>(StatusCodes.Status201Created)]
[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
public async Task<ActionResult<CashierResponse>> CreateCashier(
    CreateCashierCommand command,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### Parameter Documentation

```csharp
/// <summary>
/// Retrieves a specific cashier by their unique identifier
/// </summary>
/// <param name="id">The unique identifier (GUID) of the cashier to retrieve</param>
/// <param name="cancellationToken">Cancellation token to cancel the operation</param>
/// <returns>The cashier details if found</returns>
/// <response code="200">Returns the requested cashier</response>
/// <response code="404">Cashier with the specified ID was not found</response>
[HttpGet("{id:guid}")]
[ProducesResponseType<CashierResponse>(StatusCodes.Status200OK)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
public async Task<ActionResult<CashierResponse>> GetCashier(
    Guid id,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

## Model Documentation

### Data Transfer Objects

```csharp
/// <summary>
/// Represents a cashier in the system
/// </summary>
public record CashierResponse
{
    /// <summary>
    /// The unique identifier of the cashier
    /// </summary>
    /// <example>f47ac10b-58cc-4372-a567-0e02b2c3d479</example>
    public Guid Id { get; init; }

    /// <summary>
    /// The full name of the cashier
    /// </summary>
    /// <example>John Doe</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The email address of the cashier (must be unique)
    /// </summary>
    /// <example>john.doe@example.com</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// List of currencies this cashier can handle
    /// </summary>
    /// <example>["USD", "EUR", "GBP"]</example>
    public IReadOnlyList<string> Currencies { get; init; } = [];

    /// <summary>
    /// Indicates whether the cashier is currently active
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; init; }

    /// <summary>
    /// The timestamp when the cashier was created
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// The timestamp when the cashier was last updated
    /// </summary>
    /// <example>2024-01-15T14:45:00Z</example>
    public DateTimeOffset UpdatedAt { get; init; }
}
```

### Command Objects

```csharp
/// <summary>
/// Command to create a new cashier
/// </summary>
public record CreateCashierCommand
{
    /// <summary>
    /// The full name of the cashier (required, 2-100 characters)
    /// </summary>
    /// <example>John Doe</example>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The email address of the cashier (required, must be valid email format)
    /// </summary>
    /// <example>john.doe@example.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// List of ISO currency codes this cashier can handle (required, at least one currency)
    /// </summary>
    /// <example>["USD", "EUR"]</example>
    [Required]
    [MinLength(1)]
    public IReadOnlyList<string> Currencies { get; init; } = [];
}
```

## Advanced Documentation Features

### Complex Examples

```csharp
/// <summary>
/// Processes a payment transaction
/// </summary>
/// <param name="command">The payment command</param>
/// <returns>Payment result with transaction details</returns>
/// <remarks>
/// This endpoint processes payment transactions with the following features:
/// 
/// - Automatic currency conversion
/// - Fraud detection
/// - Real-time validation
/// - Audit trail generation
/// 
/// **Supported Payment Methods:**
/// - Credit Card (Visa, MasterCard, American Express)
/// - Bank Transfer
/// - Digital Wallets (PayPal, Apple Pay, Google Pay)
/// 
/// **Error Handling:**
/// The API returns specific error codes for different failure scenarios:
/// - 400: Invalid payment data
/// - 402: Payment declined
/// - 409: Duplicate transaction
/// - 422: Insufficient funds
/// 
/// **Sample Request:**
/// ```json
/// {
///     "amount": 100.50,
///     "currency": "USD",
///     "paymentMethod": "credit_card",
///     "cardDetails": {
///         "number": "4111111111111111",
///         "expiryMonth": 12,
///         "expiryYear": 2025,
///         "cvv": "123"
///     }
/// }
/// ```
/// 
/// **Sample Response:**
/// ```json
/// {
///     "transactionId": "txn_1234567890",
///     "status": "completed",
///     "amount": 100.50,
///     "currency": "USD",
///     "processedAt": "2024-01-15T10:30:00Z"
/// }
/// ```
/// </remarks>
[HttpPost("payments")]
public async Task<ActionResult<PaymentResponse>> ProcessPayment(ProcessPaymentCommand command)
{
    // Implementation
}
```

### Deprecation Documentation

```csharp
/// <summary>
/// Retrieves cashier by email (deprecated - use GetCashier by ID instead)
/// </summary>
/// <param name="email">The email address of the cashier</param>
/// <returns>The cashier details</returns>
/// <remarks>
/// ⚠️ **DEPRECATED**: This endpoint is deprecated and will be removed in v2.0.
/// 
/// Please use `GET /api/cashiers/{id}` instead.
/// 
/// **Migration Guide:**
/// 1. First call `GET /api/cashiers` to get all cashiers
/// 2. Find the cashier by email in the response
/// 3. Use the cashier's ID with `GET /api/cashiers/{id}`
/// </remarks>
[HttpGet("by-email/{email}")]
[Obsolete("Use GetCashier by ID instead. This endpoint will be removed in v2.0")]
public async Task<ActionResult<CashierResponse>> GetCashierByEmail(string email)
{
    // Implementation
}
```

## XML Documentation Configuration

### Project File Settings

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <!-- Enable XML documentation generation -->
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    
    <!-- Specify output path -->
    <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
    
    <!-- Treat missing XML comments as warnings -->
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>CS1591</WarningsNotAsErrors>
  </PropertyGroup>
</Project>
```

### Custom XML Documentation Service

```csharp
// Register custom XML documentation service
builder.Services.AddSingleton<IXmlDocumentationService, XmlDocumentationService>();

// Configure XML documentation paths
builder.Services.Configure<XmlDocumentationOptions>(options =>
{
    options.XmlDocumentationPaths.Add("MyApi.xml");
    options.XmlDocumentationPaths.Add("MyModels.xml");
});
```

## Best Practices

1. **Comprehensive Coverage**: Document all public APIs, parameters, and return types
2. **Meaningful Examples**: Provide realistic examples for complex types
3. **Error Documentation**: Document all possible error scenarios
4. **Deprecation Notices**: Clearly mark deprecated endpoints with migration guidance
5. **Business Context**: Include business rules and constraints in remarks
6. **Sample Data**: Use realistic sample data in examples
7. **Validation Rules**: Document validation constraints and requirements

## Troubleshooting

### Common Issues

1. **Missing Documentation**: Ensure XML generation is enabled and files are included
2. **Invalid XML**: Check for malformed XML comments
3. **Missing Examples**: Add `<example>` tags for better documentation
4. **Encoding Issues**: Ensure XML files use UTF-8 encoding

### Validation

Enable XML documentation warnings:

```xml
<PropertyGroup>
  <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

## See Also

- [OpenAPI Overview](overview.md)
- [Transformers](transformers.md)
- [API Documentation](../overview.md)