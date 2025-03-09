// Copyright (c) ABCDEG. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Billing.Cashier.Data.Entities;

[Table("CashierCurrencies")]
[PrimaryKey(nameof(CashierId), nameof(CurrencyId), nameof(EffectiveDateUtc))]
public record CashierCurrency
{
    public Guid CashierId { get; set; }

    public Guid CurrencyId { get; set; }

    public DateTime EffectiveDateUtc { get; set; }

    [MaxLength(10)]
    public string CustomCurrencyCode { get; set; } = string.Empty;

    public DateTime CreatedDateUtc { get; set; }

    [ForeignKey(nameof(CashierId))]
    public Cashier Cashier { get; set; } = null!;
}
