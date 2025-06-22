// Copyright (c) ABCDEG. All rights reserved.

using Billing.Api.Invoices.Models;
using Billing.Invoices.Commands;
using Billing.Invoices.Queries;

namespace Billing.Api.Invoices;

using InvoiceModel = Billing.Contracts.Invoices.Models.Invoice;

[ApiController]
[Route("[controller]")]
public class InvoicesController(IMessageBus bus) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceModel>>> GetInvoices([FromQuery] GetInvoicesQuery query,
        CancellationToken cancellationToken)
    {
        var invoices = await bus.InvokeQueryAsync(query, cancellationToken);

        return Ok(invoices);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceModel>> GetInvoice([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetInvoiceQuery(id);
            var invoice = await bus.InvokeQueryAsync(query, cancellationToken);

            return Ok(invoice);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceModel>> CreateInvoice([FromBody] CreateInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match<ActionResult<InvoiceModel>>(
            invoice => StatusCode(StatusCodes.Status201Created, invoice),
            errors => BadRequest(new { Errors = errors }));
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<InvoiceModel>> CancelInvoice(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelInvoiceCommand(id);
        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match<ActionResult<InvoiceModel>>(
            invoice => Ok(invoice),
            errors => BadRequest(new { Errors = errors }));
    }

    [HttpPut("{id:guid}/mark-paid")]
    public async Task<ActionResult<InvoiceModel>> MarkInvoiceAsPaid(Guid id, [FromBody] MarkInvoiceAsPaidRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MarkInvoiceAsPaidCommand(id, request.AmountPaid, request.PaymentDate);
        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match<ActionResult<InvoiceModel>>(
            invoice => Ok(invoice),
            errors => BadRequest(new { Errors = errors }));
    }

    [HttpPost("{id:guid}/simulate-payment")]
    public async Task<ActionResult> SimulatePayment(Guid id, [FromBody] SimulatePaymentRequest request,
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
