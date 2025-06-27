using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Examples;

// <json_parameters>
[DbCommand(sp: "invoices.create_invoice", nonQuery: true)]
public partial record CreateInvoiceCommand(
    Guid CashierId,
    decimal Amount,
    string Currency,
    
    [Column("metadata")]
    [JsonSerialized]
    InvoiceMetadata Metadata
) : ICommand<int>;

public record InvoiceMetadata(
    string CustomerReference,
    Dictionary<string, string> CustomFields,
    string[] Tags
);
// </json_parameters>