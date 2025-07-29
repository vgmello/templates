// Copyright (c) ABCDEG. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Billing.Api.Invoices.Models;

/// <summary>
///     Request to get a list of invoices with optional filtering and pagination
/// </summary>
public record GetInvoicesRequest
{
    /// <summary>
    ///     Maximum number of invoices to return (1-1000)
    /// </summary>
    [Range(1, 1000)]
    public int Limit { get; init; } = 50;

    /// <summary>
    ///     Number of invoices to skip for pagination
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Offset { get; init; } = 0;

    /// <summary>
    ///     Optional status filter (e.g., "Draft", "Paid", "Cancelled")
    /// </summary>
    public string? Status { get; init; }
}
