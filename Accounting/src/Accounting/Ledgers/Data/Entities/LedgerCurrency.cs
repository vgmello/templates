// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Core.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounting.Ledgers.Data.Entities;

public record LedgerCurrency : Entity
{
    public Guid LedgerBalanceId { get; set; }

    public Guid CurrencyId { get; set; }

    public DateTime EffectiveDateUtc { get; set; }

    [MaxLength(10)]
    public string CustomCurrencyCode { get; set; } = string.Empty;

    [ForeignKey(nameof(LedgerBalanceId))]
    public LedgerBalance LedgerBalance { get; set; } = null!;
}
