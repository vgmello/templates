// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.IntegrationEvents;

namespace Billing.Invoices.Commands;

public record SimulatePaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    string Currency = "USD",
    string PaymentMethod = "Credit Card",
    string PaymentReference = "SIM-REF"
) : ICommand<Result<bool>>;

public class SimulatePaymentValidator : AbstractValidator<SimulatePaymentCommand>
{
    public SimulatePaymentValidator()
    {
        RuleFor(c => c.InvoiceId).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.Currency).NotEmpty();
        RuleFor(c => c.PaymentMethod).NotEmpty();
        RuleFor(c => c.PaymentReference).NotEmpty();
    }
}

public static class SimulatePaymentCommandHandler
{
    public static Task<(Result<bool>, PaymentReceived)> Handle(SimulatePaymentCommand command, IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var paymentReceivedEvent = new PaymentReceived(
            command.InvoiceId,
            command.Amount,
            command.Currency,
            command.PaymentMethod,
            command.PaymentReference,
            DateTime.UtcNow
        );

        return Task.FromResult<(Result<bool>, PaymentReceived)>((true, paymentReceivedEvent));
    }
}
