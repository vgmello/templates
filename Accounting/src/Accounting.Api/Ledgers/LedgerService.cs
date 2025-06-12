// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Ledgers.Grpc;
using LedgerModel = Accounting.Ledgers.Grpc.Models.Ledger;
using Accounting.Contracts.Ledgers.Models;

namespace Accounting.Api.Ledgers;

public class LedgerService(IMessageBus bus, ILogger<LedgerService> logger) : LedgersService.LedgersServiceBase
{
    public override async Task<LedgerModel> GetLedger(GetLedgerRequest request, ServerCallContext context)
    {
        try
        {
            logger.LogInformation("Getting ledger {LedgerId}", request.Id);
            var result = await bus.InvokeAsync<Ledger>(new GetLedgerQuery(Guid.Parse(request.Id)), context.CancellationToken);
            return result.ToGrpc();
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ledger ID format"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting ledger {LedgerId}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<GetLedgersResponse> GetLedgers(GetLedgersRequest request, ServerCallContext context)
    {
        try
        {
            logger.LogInformation("Getting ledgers with limit {Limit}, offset {Offset}", request.Limit, request.Offset);
            var query = new GetLedgersQuery { Limit = request.Limit, Offset = request.Offset };
            var ledgers = await bus.InvokeAsync<IEnumerable<GetLedgersQuery.Result>>(query, context.CancellationToken);

            var response = new GetLedgersResponse();
            response.Ledgers.AddRange(ledgers.Select(c => c.ToGrpc()));

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting ledgers");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<LedgerModel> CreateLedger(CreateLedgerRequest request, ServerCallContext context)
    {
        try
        {
            logger.LogInformation("Creating ledger for client {ClientId}", request.ClientId);
            var command = new CreateLedgerCommand(Guid.Parse(request.ClientId), (LedgerType)request.LedgerType);
            var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

            return result.Match(
                ledger => ledger.ToGrpc(),
                errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating ledger");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}
