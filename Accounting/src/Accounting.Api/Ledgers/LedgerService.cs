// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Ledgers.Grpc;
using LedgerModel = Accounting.Ledgers.Grpc.Models.Ledger;
using Accounting.Contracts.Ledgers.Models;
using Operations.Extensions.Messaging;
using Wolverine;
using Grpc.Core;

namespace Accounting.Api.Ledgers;

public class LedgerService(IMessageBus bus) : LedgersService.LedgersServiceBase
{
    public override async Task<LedgerModel> GetLedger(GetLedgerRequest request, ServerCallContext context)
    {
        var result = await bus.InvokeAsync<Ledger>(new GetLedgerQuery(Guid.Parse(request.Id)), context.CancellationToken);

        return new LedgerModel
        {
            LedgerId = result.LedgerId.ToString(),
            ClientId = result.ClientId.ToString(),
            LedgerType = (int)result.LedgerType
        };
    }

    public override async Task<GetLedgersResponse> GetLedgers(GetLedgersRequest request, ServerCallContext context)
    {
        var query = new GetLedgersQuery { Limit = request.Limit, Offset = request.Offset };
        var ledgers = await bus.InvokeAsync<IEnumerable<GetLedgersQuery.Result>>(query, context.CancellationToken);

        var response = new GetLedgersResponse();
        response.Ledgers.AddRange(ledgers.Select(c => new LedgerModel
        {
            LedgerId = c.LedgerId.ToString(),
            ClientId = c.ClientId.ToString(),
            LedgerType = (int)c.LedgerType
        }));

        return response;
    }

    public override async Task<LedgerModel> CreateLedger(CreateLedgerRequest request, ServerCallContext context)
    {
        var command = new CreateLedgerCommand(Guid.Parse(request.ClientId), (LedgerType)request.LedgerType);
        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            ledger => new LedgerModel
            {
                LedgerId = ledger.LedgerId.ToString(),
                ClientId = ledger.ClientId.ToString(),
                LedgerType = (int)ledger.LedgerType
            },
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }
}
