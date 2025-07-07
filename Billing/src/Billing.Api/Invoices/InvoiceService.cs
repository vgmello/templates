// Copyright (c) ABCDEG. All rights reserved.

using Billing.Api.Invoices.Mappers;
using Billing.Invoices.Commands;
using Billing.Invoices.Grpc;
using Billing.Invoices.Queries;
using InvoiceModel = Billing.Invoices.Grpc.Models.Invoice;

namespace Billing.Api.Invoices;

public class InvoiceService(IMessageBus bus) : InvoicesService.InvoicesServiceBase
{
    public override async Task<InvoiceModel> GetInvoice(GetInvoiceRequest request, ServerCallContext context)
    {
        var query = new GetInvoiceQuery(Guid.Parse(request.Id));
        var result = await bus.InvokeQueryAsync(query, context.CancellationToken);

        return result.Match(
            invoice => invoice.ToGrpc(),
            errors => throw new RpcException(new Status(StatusCode.NotFound, string.Join("; ", errors))));
    }

    public override async Task<GetInvoicesResponse> GetInvoices(GetInvoicesRequest request, ServerCallContext context)
    {
        var query = new GetInvoicesQuery(request.Limit, request.Offset, request.Status);
        var invoices = await bus.InvokeQueryAsync(query, context.CancellationToken);

        var invoicesGrpc = invoices.Select(i => i.ToGrpc());

        return new GetInvoicesResponse
        {
            Invoices = { invoicesGrpc }
        };
    }

    public override async Task<InvoiceModel> CreateInvoice(CreateInvoiceRequest request, ServerCallContext context)
    {
        var dueDate = request.DueDate?.ToDateTime();
        var cashierId = string.IsNullOrEmpty(request.CashierId) ? (Guid?)null : Guid.Parse(request.CashierId);

        var command = new CreateInvoiceCommand(
            request.Name,
            (decimal)request.Amount,
            request.Currency,
            dueDate,
            cashierId);

        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            invoice => invoice.ToGrpc(),
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }

    public override async Task<InvoiceModel> CancelInvoice(CancelInvoiceRequest request, ServerCallContext context)
    {
        var command = new CancelInvoiceCommand(Guid.Parse(request.InvoiceId));
        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            invoice => invoice.ToGrpc(),
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }

    public override async Task<InvoiceModel> MarkInvoiceAsPaid(MarkInvoiceAsPaidRequest request, ServerCallContext context)
    {
        var paymentDate = request.PaymentDate?.ToDateTime();
        var command = new MarkInvoiceAsPaidCommand(
            Guid.Parse(request.InvoiceId),
            (decimal)request.AmountPaid,
            paymentDate);

        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            invoice => invoice.ToGrpc(),
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }

    public override async Task<SimulatePaymentResponse> SimulatePayment(SimulatePaymentRequest request, ServerCallContext context)
    {
        var command = new SimulatePaymentCommand(
            Guid.Parse(request.InvoiceId),
            (decimal)request.Amount,
            request.Currency,
            request.PaymentMethod ?? "Credit Card",
            request.PaymentReference ?? $"SIM-{Guid.NewGuid():N}"[..8]
        );

        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            _ => new SimulatePaymentResponse { Message = "Payment simulation triggered successfully" },
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }
}
