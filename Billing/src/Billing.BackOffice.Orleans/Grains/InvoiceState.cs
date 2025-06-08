using Orleans.Serialization;

namespace Billing.BackOffice.Orleans.Grains;

[GenerateSerializer]
public sealed class InvoiceState
{
    [Id(0)]
    public decimal Amount { get; set; }
    [Id(1)]
    public bool Paid { get; set; }
}
