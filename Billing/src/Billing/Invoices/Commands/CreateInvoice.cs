// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.IntegrationEvents;
using Billing.Invoices.Contracts.Models;

namespace Billing.Invoices.Commands;

using InvoiceModel = Invoice;

public record CreateInvoiceCommand(
    string Name,
    decimal Amount,
    string? Currency = "",
    DateTime? DueDate = null,
    Guid? CashierId = null
) : ICommand<Result<InvoiceModel>>;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Name).MaximumLength(100);
        RuleFor(c => c.Name).MinimumLength(2);
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.Currency).MaximumLength(3).When(c => !string.IsNullOrEmpty(c.Currency));
    }
}

public static partial class CreateInvoiceCommandHandler
{
    [DbCommand(sp: "billing.invoices_create", nonQuery: true)]
    public partial record InsertInvoiceCommand(
        Guid InvoiceId,
        string Name,
        string Status,
        decimal Amount,
        string? Currency,
        DateTime? DueDate,
        Guid? CashierId
    ) : ICommand<int>;

    public static async Task<(Result<InvoiceModel>, InvoiceCreated)> Handle(CreateInvoiceCommand command, IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var invoiceId = Guid.CreateVersion7();
        const string status = "Draft";

        var insertCommand = new InsertInvoiceCommand(
            invoiceId,
            command.Name,
            status,
            command.Amount,
            command.Currency,
            command.DueDate,
            command.CashierId
        );

        await messaging.InvokeCommandAsync(insertCommand, cancellationToken);

        var result = new InvoiceModel
        {
            InvoiceId = invoiceId,
            Name = command.Name,
            Status = status,
            Amount = command.Amount,
            Currency = command.Currency,
            DueDate = command.DueDate,
            CashierId = command.CashierId,
            CreatedDateUtc = DateTime.UtcNow,
            UpdatedDateUtc = DateTime.UtcNow,
            Version = 1
        };

        var createdEvent = new InvoiceCreated(result);

        return (result, createdEvent);
    }
}
