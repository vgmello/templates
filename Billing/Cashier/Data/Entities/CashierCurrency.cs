using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Cashier.Data.Entities;

[Table("CashierCurrencies")]
public record CashierCurrency
{
    [Key]
    public Guid CurrencyId { get; set; }

    [Key]
    public Guid CashierId { get; set; }

    [Key]
    public DateTime EffectiveDateUtc { get; set; }

    [MaxLength(10)]
    public string CustomCurrencyCode { get; set; } = string.Empty;

    public DateTime CreatedDateUtc { get; set; }

    [ForeignKey(nameof(CashierId))]
    public Cashier Cashier { get; set; } = null!;
}
