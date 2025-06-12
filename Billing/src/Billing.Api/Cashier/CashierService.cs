// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Grpc;
using CashierModel = Billing.Cashier.Grpc.Models.Cashier;

namespace Billing.Api.Cashier;

public class CashierService(IMessageBus bus, ILogger<CashierService> logger) : CashiersService.CashiersServiceBase
{
    public override async Task<CashierModel> GetCashier(GetCashierRequest request, ServerCallContext context)
    {
        try
        {
            logger.LogInformation("Getting cashier {CashierId}", request.Id);
            var result = await bus.InvokeQueryAsync(new GetCashierQuery(Guid.Parse(request.Id)), context.CancellationToken);
            return result.ToGrpc();
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid cashier ID format"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting cashier {CashierId}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<GetCashiersResponse> GetCashiers(GetCashiersRequest request, ServerCallContext context)
    {
        try
        {
            logger.LogInformation("Getting cashiers with limit {Limit}, offset {Offset}", request.Limit, request.Offset);
            var query = new GetCashiersQuery { Limit = request.Limit, Offset = request.Offset };
            var cashiers = await bus.InvokeQueryAsync(query, context.CancellationToken);

            var cashiersGrpc = cashiers.Select(c => c.ToGrpc());

            return new GetCashiersResponse
            {
                Cashiers = { cashiersGrpc }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting cashiers");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<CashierModel> CreateCashier(CreateCashierRequest request, ServerCallContext context)
    {
        try
        {
            logger.LogInformation("Creating cashier with name {Name} and email {Email}", request.Name, request.Email);
            
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Name is required"));
            
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Email is required"));

            var command = new CreateCashierCommand(request.Name, request.Email);
            var result = await bus.InvokeCommandAsync(command, context.CancellationToken);

            return result.Match(
                cashier => cashier.ToGrpc(),
                errors => throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join("; ", errors))));
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating cashier");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}
