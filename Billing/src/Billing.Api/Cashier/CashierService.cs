// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Grpc;
using CashierModel = Billing.Cashier.Grpc.Models.Cashier;
using Operations.Extensions.Messaging;
using Wolverine;
using Grpc.Core;

namespace Billing.Api.Cashier;

public class CashierService(IMessageBus bus) : Cashiers.CashiersBase
{
    public override async Task<CashierModel> GetCashier(GetCashierRequest request, ServerCallContext context)
    {
        var result = await bus.InvokeAsync<Billing.Contracts.Cashier.Models.Cashier>(
            new GetCashierQuery(Guid.Parse(request.Id)), context.CancellationToken);

        return new CashierModel
        {
            CashierId = result.CashierId.ToString(),
            CashierNumber = result.CashierNumber,
            Name = result.Name,
            Email = result.Email
        };
    }

    public override async Task<GetCashiersResponse> GetCashiers(GetCashiersRequest request, ServerCallContext context)
    {
        var query = new GetCashiersQuery { Limit = request.Limit, Offset = request.Offset };
        var cashiers = await bus.InvokeAsync<IEnumerable<GetCashiersQuery.Result>>(query, context.CancellationToken);

        var response = new GetCashiersResponse();
        response.Cashiers.AddRange(cashiers.Select(c => new CashierModel
        {
            CashierId = c.CashierId.ToString(),
            Name = c.Name
        }));

        return response;
    }

    public override async Task<CashierModel> CreateCashier(CreateCashierRequest request, ServerCallContext context)
    {
        var command = new CreateCashierCommand(request.Name, request.Email);
        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            cashier => new CashierModel
            {
                CashierId = cashier.CashierId.ToString(),
                CashierNumber = cashier.CashierNumber,
                Name = cashier.Name,
                Email = cashier.Email
            },
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }
}
