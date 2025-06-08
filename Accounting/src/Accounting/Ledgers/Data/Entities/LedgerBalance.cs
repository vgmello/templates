using Accounting.Core.Data;
using Accounting.Contracts.Ledgers.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounting.Ledgers.Data.Entities;

[Table("ledger_balances")]
public record LedgerBalance : Entity
{
    public Guid LedgerBalanceId { get; set; }

    public Guid ClientId { get; set; }

    public LedgerType LedgerType { get; set; }

    public DateOnly BalanceDate { get; set; }
}
