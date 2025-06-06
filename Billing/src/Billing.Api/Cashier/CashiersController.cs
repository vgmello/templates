// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Messaging;
using Wolverine;

namespace Billing.Api.Cashier;

[ApiController]
[Route("[controller]")]
public class CashiersController(ILogger<CashiersController> logger, IMessageBus bus) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Contracts.Cashier.Models.Cashier>> GetCashier([FromRoute] Guid id)
    {
        var cashier = await bus.InvokeAsync<Contracts.Cashier.Models.Cashier>(new GetCashierQuery(id));

        return cashier;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCashiersQuery.Result>>> GetCashiers([FromQuery] GetCashiersQuery query)
    {
        var cashiers = await bus.InvokeAsync<IEnumerable<GetCashiersQuery.Result>>(query);

        return Ok(cashiers);
    }

    [HttpPost]
    public async Task<ActionResult<Contracts.Cashier.Models.Cashier>> CreateCashier(CreateCashierCommand command)
    {
        var commandResult = await bus.InvokeCommandAsync(command);

        return commandResult.Match(
            cashier => StatusCode(StatusCodes.Status201Created, cashier),
            errors => BadRequest(new { Errors = errors }));
    }

    [HttpGet("fake-error")]
    public Task<ActionResult<Contracts.Cashier.Models.Cashier>> FakeError() => throw new DivideByZeroException("Fake error");

    [HttpGet("id/{id}")]
    public async Task<ActionResult<Contracts.Cashier.Models.Cashier>> GetCashierById([FromRoute] int id)
    {
        using var loggerScope = logger.BeginScope(new Dictionary<string, object?> { ["Id"] = id });

        var cashier = await bus.InvokeAsync<Contracts.Cashier.Models.Cashier>(new GetCashierQuery(Guid.NewGuid()));

        return cashier;
    }
}
