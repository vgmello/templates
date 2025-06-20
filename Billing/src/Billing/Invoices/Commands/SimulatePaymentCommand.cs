// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Invoices.IntegrationEvents;

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
    public static Task<(Result<bool>, PaymentReceivedEvent)> Handle(
        SimulatePaymentCommand command,
        IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        // This is a fake method to simulate receiving a payment
        // In a real system, this would be triggered by an external payment processor
        
        var paymentReceivedEvent = new PaymentReceivedEvent(
            command.InvoiceId,
            command.Amount,
            command.Currency,
            command.PaymentMethod,
            command.PaymentReference,
            DateTime.UtcNow
        );

        // Return success and the event that will be published
        return Task.FromResult<(Result<bool>, PaymentReceivedEvent)>((true, paymentReceivedEvent));
    }
}