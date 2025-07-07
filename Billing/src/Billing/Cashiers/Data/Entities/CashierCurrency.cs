// Copyright (c) ABCDEG. All rights reserved.

using Billing.Core.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Cashiers.Data.Entities;

public record CashierCurrency : Entity
{
    public Guid CashierId { get; set; }

    public Guid CurrencyId { get; set; }

    public DateTime EffectiveDateUtc { get; set; }

    [MaxLength(10)]
    public string CustomCurrencyCode { get; set; } = string.Empty;

    [ForeignKey(nameof(CashierId))]
    public Cashier Cashier { get; set; } = null!;
}
