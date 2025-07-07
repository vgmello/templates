// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.IntegrationEvents;
using Billing.Invoices.Contracts.Models;
using FluentValidation.Results;

namespace Billing.Invoices.Commands;

using InvoiceModel = Invoice;

public record CancelInvoiceCommand(Guid InvoiceId) : ICommand<Result<InvoiceModel>>;

public class CancelInvoiceValidator : AbstractValidator<CancelInvoiceCommand>
{
    public CancelInvoiceValidator()
    {
        RuleFor(c => c.InvoiceId).NotEmpty();
    }
}

public static partial class CancelInvoiceCommandHandler
{
    [DbCommand(sp: "billing.invoices_cancel", nonQuery: true)]
    public partial record CancelInvoiceDbCommand(Guid InvoiceId) : ICommand<int>;

    public static async Task<(Result<InvoiceModel>, InvoiceCancelled?)> Handle(CancelInvoiceCommand command, IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var cancelDbCommand = new CancelInvoiceDbCommand(command.InvoiceId);

        var rowsAffected = await messaging.InvokeCommandAsync(cancelDbCommand, cancellationToken);

        if (rowsAffected == 0)
        {
            var failures = new List<ValidationFailure> { new("InvoiceId", "Invoice not found or cannot be cancelled") };

            return (failures, null);
        }

        // In a real scenario, we'd need to fetch the updated invoice data
        var result = new InvoiceModel
        {
            InvoiceId = command.InvoiceId,
            Name = "Cancelled Invoice",
            Status = "Cancelled",
            Amount = 0,
            CreatedDateUtc = DateTime.UtcNow,
            UpdatedDateUtc = DateTime.UtcNow,
            Version = 99
        };

        var cancelledEvent = new InvoiceCancelled(command.InvoiceId);

        return (result, cancelledEvent);
    }
}
