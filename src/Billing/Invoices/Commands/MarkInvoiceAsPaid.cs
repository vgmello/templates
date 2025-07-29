// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.IntegrationEvents;
using Billing.Invoices.Contracts.Models;
using Billing.Invoices.Data;
using FluentValidation.Results;
using Operations.Extensions.Abstractions.Dapper;

namespace Billing.Invoices.Commands;

public record MarkInvoiceAsPaidCommand(
    Guid TenantId,
    Guid InvoiceId,
    int Version,
    decimal AmountPaid,
    DateTime? PaymentDate = null) : ICommand<Result<Invoice>>;

public class MarkInvoiceAsPaidValidator : AbstractValidator<MarkInvoiceAsPaidCommand>
{
    public MarkInvoiceAsPaidValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.InvoiceId).NotEmpty();
        RuleFor(c => c.Version).GreaterThanOrEqualTo(0);
        RuleFor(c => c.AmountPaid).GreaterThan(0);
    }
}

public static partial class MarkInvoiceAsPaidCommandHandler
{
    [DbCommand(fn: "$billing.invoices_mark_paid")]
    public partial record MarkInvoiceAsPaidDbCommand(Guid TenantId, Guid InvoiceId, int Version, decimal AmountPaid, DateTime PaymentDate)
        : ICommand<Data.Entities.Invoice?>;

    public static async Task<(Result<Invoice>, InvoicePaid?)> Handle(
        MarkInvoiceAsPaidCommand command, IMessageBus messaging, CancellationToken cancellationToken)
    {
        var paymentDate = command.PaymentDate ?? DateTime.UtcNow;
        var dbCommand =
            new MarkInvoiceAsPaidDbCommand(command.TenantId, command.InvoiceId, command.Version, command.AmountPaid, paymentDate);

        var updatedInvoice = await messaging.InvokeCommandAsync(dbCommand, cancellationToken);

        if (updatedInvoice is null)
        {
            var failures = new List<ValidationFailure>
            {
                new("Version", "Invoice not found, already paid, or was modified by another user. " +
                               "Please refresh and try again.")
            };

            return (failures, null);
        }

        var result = updatedInvoice.ToModel();
        var paidEvent = new InvoicePaid(updatedInvoice.TenantId, result);

        return (result, paidEvent);
    }
}
