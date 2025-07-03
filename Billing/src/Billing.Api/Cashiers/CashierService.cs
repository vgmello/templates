// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Commands;
using Billing.Cashiers.Grpc;
using Billing.Cashiers.Queries;
using Google.Protobuf.WellKnownTypes;
using CashierModel = Billing.Cashiers.Grpc.Models.Cashier;

namespace Billing.Api.Cashiers;

public class CashierService(IMessageBus bus) : CashiersService.CashiersServiceBase
{
    public override async Task<CashierModel> GetCashier(GetCashierRequest request, ServerCallContext context)
    {
        var result = await bus.InvokeQueryAsync(new GetCashierQuery(Guid.Parse(request.Id)), context.CancellationToken);

        return result.ToGrpc();
    }

    public override async Task<GetCashiersResponse> GetCashiers(GetCashiersRequest request, ServerCallContext context)
    {
        var query = new GetCashiersQuery { Limit = request.Limit, Offset = request.Offset };
        var cashiers = await bus.InvokeQueryAsync(query, context.CancellationToken);

        var cashiersGrpc = cashiers.Select(c => c.ToGrpc());

        return new GetCashiersResponse
        {
            Cashiers = { cashiersGrpc }
        };
    }

    public override async Task<CashierModel> CreateCashier(CreateCashierRequest request, ServerCallContext context)
    {
        var command = new CreateCashierCommand(Guid.Empty, request.Name, request.Email);
        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            cashier => cashier.ToGrpc(),
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }

    public override async Task<CashierModel> UpdateCashier(UpdateCashierRequest request, ServerCallContext context)
    {
        var command = new UpdateCashierCommand(Guid.Parse(request.CashierId), request.Name, request.Email);
        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            cashier => cashier.ToGrpc(),
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }

    public override async Task<Empty> DeleteCashier(DeleteCashierRequest request, ServerCallContext context)
    {
        var command = new DeleteCashierCommand(Guid.Parse(request.CashierId));
        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            _ => new Empty(),
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }
}
