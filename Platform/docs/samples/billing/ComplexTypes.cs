using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;

namespace Billing.Examples;

// <complex_types>
[DbCommand(sp: "invoices.create_invoice", nonQuery: true)]
public partial record CreateInvoiceCommand(
    Guid CashierId,
    decimal Amount,
    string Currency,
    
    // Nullable reference type
    string? Description,
    
    // Array parameter
    string[] Tags,
    
    // Nullable value type
    DateTimeOffset? DueDate,
    
    // Enum (converted to string/int)
    InvoiceStatus Status
) : ICommand<int>;

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    Cancelled
}
// </complex_types>