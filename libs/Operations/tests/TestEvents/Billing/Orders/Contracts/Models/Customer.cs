namespace Billing.Orders.Contracts.Models;

/// <summary>Represents customer information for order processing</summary>
public record Customer
{
    /// <summary>Unique identifier for the customer</summary>
    public required Guid CustomerId { get; init; }

    /// <summary>Full name of the customer</summary>
    public required string Name { get; init; }

    /// <summary>Email address for order notifications</summary>
    public required string Email { get; init; }

    /// <summary>Complete billing address information</summary>
    public required Address BillingAddress { get; init; }
}