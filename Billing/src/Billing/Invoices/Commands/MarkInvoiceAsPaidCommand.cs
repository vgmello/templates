// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Invoices.IntegrationEvents;
using FluentValidation.Results;

namespace Billing.Invoices.Commands;

using InvoiceModel = Billing.Contracts.Invoices.Models.Invoice;

public record MarkInvoiceAsPaidCommand(Guid InvoiceId, decimal AmountPaid, DateTime? PaymentDate = null) : ICommand<Result<InvoiceModel>>;

public class MarkInvoiceAsPaidValidator : AbstractValidator<MarkInvoiceAsPaidCommand>
{
    public MarkInvoiceAsPaidValidator()
    {
        RuleFor(c => c.InvoiceId).NotEmpty();
        RuleFor(c => c.AmountPaid).GreaterThan(0);
    }
}

public static partial class MarkInvoiceAsPaidCommandHandler
{
    [DbCommand(sp: "billing.mark_invoice_as_paid", nonQuery: true)]
    public partial record MarkInvoiceAsPaidDbCommand(Guid InvoiceId, decimal AmountPaid, DateTime PaymentDate) : ICommand<int>;

    public static async Task<(Result<InvoiceModel>, InvoicePaidEvent?)> Handle(
        MarkInvoiceAsPaidCommand command, IMessageBus messaging, CancellationToken cancellationToken)
    {
        var paymentDate = command.PaymentDate ?? DateTime.UtcNow;
        var markPaidDbCommand = new MarkInvoiceAsPaidDbCommand(command.InvoiceId, command.AmountPaid, paymentDate);

        var rowsAffected = await messaging.InvokeCommandAsync(markPaidDbCommand, cancellationToken);

        if (rowsAffected == 0)
        {
            var failures = new List<ValidationFailure> { new("InvoiceId", "Invoice not found or already paid") };

            return (failures, null);
        }

        // In a real scenario, we'd need to fetch the updated invoice data
        var result = new InvoiceModel
        {
            InvoiceId = command.InvoiceId,
            Name = "Paid Invoice", // This should be fetched from DB
            Status = "Paid",
            Amount = command.AmountPaid,
            CreatedDateUtc = DateTime.UtcNow, // This should be fetched from DB
            UpdatedDateUtc = DateTime.UtcNow,
            Version = 1 // This should be incremented
        };

        var paidEvent = new InvoicePaidEvent(command.InvoiceId, command.AmountPaid, paymentDate);

        return (result, paidEvent);
    }
}
