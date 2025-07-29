// Copyright (c) ABCDEG. All rights reserved.

using System.Text.Json.Serialization;

namespace Billing.Api.Cashiers.Models;

/// <summary>
///     Request to update an existing cashier
/// </summary>
public record UpdateCashierRequest
{
    /// <summary>
    ///     The updated name of the cashier
    /// </summary>
    [JsonRequired]
    public required string Name { get; init; }

    /// <summary>
    ///     The updated email address of the cashier (can be null to leave unchanged)
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    ///     The current version of the cashier for optimistic concurrency control
    /// </summary>
    [JsonRequired]
    public required int Version { get; init; }
}
