// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.IntegrationEvents;
using Billing.Invoices.Contracts.Models;
using Billing.Invoices.Data;
using FluentValidation.Results;
using Operations.Extensions.Abstractions.Dapper;

namespace Billing.Invoices.Commands;

public record CancelInvoiceCommand(Guid TenantId, Guid InvoiceId, int Version) : ICommand<Result<Invoice>>;

public class CancelInvoiceValidator : AbstractValidator<CancelInvoiceCommand>
{
    public CancelInvoiceValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.InvoiceId).NotEmpty();
        RuleFor(c => c.Version).GreaterThanOrEqualTo(0);
    }
}

public static partial class CancelInvoiceCommandHandler
{
    [DbCommand(fn: "$billing.invoices_cancel")]
    public partial record DbCommand(Guid TenantId, Guid InvoiceId, int Version) : ICommand<Data.Entities.Invoice?>;

    public static async Task<(Result<Invoice>, InvoiceCancelled?)> Handle(CancelInvoiceCommand command, IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var dbCommand = new DbCommand(command.TenantId, command.InvoiceId, command.Version);
        var updatedInvoice = await messaging.InvokeCommandAsync(dbCommand, cancellationToken);

        if (updatedInvoice is null)
        {
            var failures = new List<ValidationFailure>
            {
                new("Version", "Invoice not found, cannot be cancelled, or was modified by another user. " +
                               "Please refresh and try again.")
            };

            return (failures, null);
        }

        var result = updatedInvoice.ToModel();
        var cancelledEvent = new InvoiceCancelled(command.TenantId, command.InvoiceId);

        return (result, cancelledEvent);
    }
}
