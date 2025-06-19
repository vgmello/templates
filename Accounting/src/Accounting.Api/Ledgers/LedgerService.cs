// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Ledgers.Grpc;
using LedgerModel = Accounting.Ledgers.Grpc.Models.Ledger;
using Accounting.Contracts.Ledgers.Models;

namespace Accounting.Api.Ledgers;

public class LedgerService(IMessageBus bus) : LedgersService.LedgersServiceBase
{
    public override async Task<LedgerModel> GetLedger(GetLedgerRequest request, ServerCallContext context)
    {
        var result = await bus.InvokeAsync<Ledger>(new GetLedgerQuery(Guid.Parse(request.Id)), context.CancellationToken);

        return result.ToGrpc();
    }

    public override async Task<GetLedgersResponse> GetLedgers(GetLedgersRequest request, ServerCallContext context)
    {
        var query = new GetLedgersQuery { Limit = request.Limit, Offset = request.Offset };
        var ledgers = await bus.InvokeAsync<IEnumerable<GetLedgersQuery.Result>>(query, context.CancellationToken);

        var response = new GetLedgersResponse();
        response.Ledgers.AddRange(ledgers.Select(c => c.ToGrpc()));

        return response;
    }

    public override async Task<LedgerModel> CreateLedger(CreateLedgerRequest request, ServerCallContext context)
    {
        var command = new CreateLedgerCommand(Guid.Parse(request.ClientId), (LedgerType)request.LedgerType);
        var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

        return result.Match(
            ledger => ledger.ToGrpc(),
            errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
    }
}
