// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Grpc;
using CashierModel = Billing.Cashier.Grpc.Models.Cashier;

namespace Billing.Api.Cashier;

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
        var command = new CreateCashierCommand(request.Name, request.Email);
        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            cashier => cashier.ToGrpc(),
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }
}
