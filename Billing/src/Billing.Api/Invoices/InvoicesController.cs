// Copyright (c) ABCDEG. All rights reserved.

using Billing.Api.Invoices.Models;
using Billing.Invoices.Commands;
using Billing.Invoices.Contracts.Models;
using Billing.Invoices.Queries;

namespace Billing.Api.Invoices;

using InvoiceModel = Invoice;

/// <summary>
///     Manages invoice operations such as retrieval, creation, cancellation, and payment processing.
/// </summary>
[ApiController]
[Route("[controller]")]
public class InvoicesController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    ///     Retrieves a list of invoices with optional filtering and pagination
    /// </summary>
    /// <param name="query">Query parameters for filtering invoices by status, date range, cashier, etc.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of invoices matching the specified criteria</returns>
    /// <response code="200">Returns the list of invoices</response>
    /// <response code="400">If query parameters are invalid</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceModel>>> GetInvoices([FromQuery] GetInvoicesQuery query,
        CancellationToken cancellationToken)
    {
        var invoices = await bus.InvokeQueryAsync(query, cancellationToken);

        return Ok(invoices);
    }

    /// <summary>
    ///     Retrieves a specific invoice by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the invoice</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The invoice details if found</returns>
    /// <response code="200" />
    /// <response code="404">If the invoice is not found</response>
    /// <response code="400">If the provided ID is invalid</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoiceModel>> GetInvoice([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetInvoiceQuery(id);
        var queryResult = await bus.InvokeQueryAsync(query, cancellationToken);

        return queryResult.Match<ActionResult<InvoiceModel>>(
            invoice => Ok(invoice),
            errors => NotFound(new { Errors = errors }));
    }

    /// <summary>
    ///     Creates a new invoice in the system
    /// </summary>
    /// <param name="command">The invoice creation request containing name, amount, currency, due date, and optional cashier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created invoice details</returns>
    /// <response code="201">Invoice created successfully</response>
    /// <response code="400">If the request data is invalid or validation fails</response>
    /// <response code="404">If the specified cashier is not found</response>
    [HttpPost]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoiceModel>> CreateInvoice([FromBody] CreateInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match<ActionResult<InvoiceModel>>(
            invoice => StatusCode(StatusCodes.Status201Created, invoice),
            errors => BadRequest(new { Errors = errors }));
    }

    /// <summary>
    ///     Cancels an existing invoice, preventing further modifications
    /// </summary>
    /// <param name="id">The unique identifier of the invoice to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated invoice details with cancelled status</returns>
    /// <response code="200">Invoice cancelled successfully</response>
    /// <response code="400">If the invoice cannot be cancelled (already paid or cancelled)</response>
    /// <response code="404">If the invoice is not found</response>
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoiceModel>> CancelInvoice([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelInvoiceCommand(id);
        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match<ActionResult<InvoiceModel>>(
            invoice => Ok(invoice),
            errors => BadRequest(new { Errors = errors }));
    }

    /// <summary>
    ///     Marks an invoice as paid with the specified payment amount and date
    /// </summary>
    /// <param name="id">The unique identifier of the invoice to mark as paid</param>
    /// <param name="request">Payment details including amount and optional payment date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated invoice details with paid status</returns>
    /// <response code="200">Invoice marked as paid successfully</response>
    /// <response code="400">If the payment amount is insufficient or invoice is already paid/cancelled</response>
    /// <response code="404">If the invoice is not found</response>
    [HttpPut("{id:guid}/mark-paid")]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvoiceModel>> MarkInvoiceAsPaid([FromRoute] Guid id, [FromBody] MarkInvoiceAsPaidRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MarkInvoiceAsPaidCommand(id, request.AmountPaid, request.PaymentDate);
        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match<ActionResult<InvoiceModel>>(
            invoice => Ok(invoice),
            errors => BadRequest(new { Errors = errors }));
    }

    /// <summary>
    ///     Simulates a payment for testing purposes, triggering payment processing workflow
    /// </summary>
    /// <param name="id">The unique identifier of the invoice to simulate payment for</param>
    /// <param name="request">Payment simulation details including amount, currency, method, and reference</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation that payment simulation was triggered</returns>
    /// <response code="200">Payment simulation triggered successfully</response>
    /// <response code="400">If the payment details are invalid or invoice cannot be paid</response>
    /// <response code="404">If the invoice is not found</response>
    [HttpPost("{id:guid}/simulate-payment")]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    [Tags("Testing")]
    public async Task<ActionResult> SimulatePayment([FromRoute] Guid id, [FromBody] SimulatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SimulatePaymentCommand(
            id,
            request.Amount,
            request.Currency ?? "USD",
            request.PaymentMethod ?? "Credit Card",
            request.PaymentReference ?? $"SIM-{Guid.NewGuid():N}"[..8]
        );

        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match<ActionResult>(
            _ => Ok(new { Message = "Payment simulation triggered successfully" }),
            errors => BadRequest(new { Errors = errors }));
    }
}
