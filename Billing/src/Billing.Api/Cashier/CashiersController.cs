// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Api.Cashier;

[ApiController]
[Route("[controller]")]
public class CashiersController(IMessageBus bus) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Contracts.Cashier.Models.Cashier>> GetCashier([FromRoute] Guid id)
    {
        var cashier = await bus.InvokeQueryAsync(new GetCashierQuery(id));

        return cashier;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCashiersQuery.Result>>> GetCashiers([FromQuery] GetCashiersQuery query)
    {
        var cashiers = await bus.InvokeQueryAsync(query);

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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Contracts.Cashier.Models.Cashier>> UpdateCashier([FromRoute] Guid id, UpdateCashierCommand command)
    {
        var commandWithId = command with { CashierId = id };
        var commandResult = await bus.InvokeCommandAsync(commandWithId);

        return commandResult.Match<ActionResult<Contracts.Cashier.Models.Cashier>>(
            cashier => Ok(cashier),
            errors => BadRequest(new { Errors = errors }));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteCashier([FromRoute] Guid id)
    {
        var command = new DeleteCashierCommand(id);
        var commandResult = await bus.InvokeCommandAsync(command);

        return commandResult.Match<ActionResult>(
            _ => NoContent(),
            errors => BadRequest(new { Errors = errors }));
    }

    [HttpGet("fake-error")]
    public Task<ActionResult<Contracts.Cashier.Models.Cashier>> FakeError() => throw new DivideByZeroException("Fake error");
}
