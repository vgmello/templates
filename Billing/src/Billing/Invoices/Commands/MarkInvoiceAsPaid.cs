// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.IntegrationEvents;
using Billing.Invoices.Contracts.Models;
using Billing.Invoices.Queries;
using FluentValidation.Results;

namespace Billing.Invoices.Commands;

using InvoiceModel = Invoice;

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
    [DbCommand(sp: "billing.invoices_mark_paid", nonQuery: true)]
    public partial record MarkInvoiceAsPaidDbCommand(Guid InvoiceId, decimal AmountPaid, DateTime PaymentDate) : ICommand<int>;

    public static async Task<(Result<InvoiceModel>, InvoicePaid?)> Handle(
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

        // Fetch the updated invoice from the database
        var getInvoiceQuery = new GetInvoiceQuery(command.InvoiceId);
        var invoiceResult = await messaging.InvokeQueryAsync(getInvoiceQuery, cancellationToken);

        if (invoiceResult.IsT1)
        {
            var failures = new List<ValidationFailure> { new("InvoiceId", "Failed to retrieve updated invoice") };

            return (failures, null);
        }

        var updatedInvoice = invoiceResult.AsT0;

        var paidEvent = new InvoicePaid
        {
            TenantId = "some tenant-id",
            InvoiceId = command.InvoiceId,
            AmountPaid = command.AmountPaid,
            PaymentDate = paymentDate
        };

        return (updatedInvoice, paidEvent);
    }
}
