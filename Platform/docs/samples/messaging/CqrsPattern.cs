using Operations.Extensions.Abstractions.Messaging;

namespace Platform.Samples.Messaging;

// #region CommandDefinition
/// <summary>
/// Command to create a new invoice
/// </summary>
public record CreateInvoiceCommand(
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTime DueDate
) : ICommand<Guid>;
// #endregion

// #region QueryDefinition
/// <summary>
/// Query to get invoice details
/// </summary>
public record GetInvoiceQuery(
    Guid InvoiceId
) : IQuery<InvoiceDetails?>;
// #endregion

// #region CommandHandler
/// <summary>
/// Handler for creating invoices
/// </summary>
public class CreateInvoiceHandler
{
    private readonly IInvoiceRepository _repository;
    private readonly IMessageBus _messageBus;

    public CreateInvoiceHandler(IInvoiceRepository repository, IMessageBus messageBus)
    {
        _repository = repository;
        _messageBus = messageBus;
    }

    public async Task<Guid> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CustomerId = command.CustomerId,
            Amount = command.Amount,
            Currency = command.Currency,
            DueDate = command.DueDate,
            Status = InvoiceStatus.Draft
        };

        await _repository.CreateAsync(invoice, cancellationToken);

        // Publish event for other services
        await _messageBus.PublishAsync(new InvoiceCreated(
            invoice.Id,
            invoice.CustomerId,
            invoice.Amount,
            invoice.Currency
        ), cancellationToken);

        return invoice.Id;
    }
}
// #endregion

// #region QueryHandler
/// <summary>
/// Handler for retrieving invoice details
/// </summary>
public class GetInvoiceHandler
{
    private readonly IInvoiceRepository _repository;

    public GetInvoiceHandler(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<InvoiceDetails?> Handle(GetInvoiceQuery query, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(query.InvoiceId, cancellationToken);
        
        return invoice is null ? null : new InvoiceDetails(
            invoice.Id,
            invoice.CustomerId,
            invoice.Amount,
            invoice.Currency,
            invoice.DueDate,
            invoice.Status
        );
    }
}
// #endregion

// #region EventDefinition
/// <summary>
/// Event published when an invoice is created
/// </summary>
public record InvoiceCreated(
    Guid InvoiceId,
    Guid CustomerId,
    decimal Amount,
    string Currency
);
// #endregion

// Supporting types
public class Invoice
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
}

public enum InvoiceStatus { Draft, Pending, Paid, Cancelled }

public record InvoiceDetails(
    Guid Id,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTime DueDate,
    InvoiceStatus Status
);

public interface IInvoiceRepository
{
    Task CreateAsync(Invoice invoice, CancellationToken cancellationToken);
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

public interface IMessageBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken);
}