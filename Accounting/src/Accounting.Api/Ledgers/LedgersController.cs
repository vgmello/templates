// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.Api.Ledgers;

[ApiController]
[Route("[controller]")]
public class LedgersController(IMessageBus bus) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Contracts.Ledgers.Models.Ledger>> GetLedger([FromRoute] Guid id)
    {
        var ledger = await bus.InvokeAsync<Contracts.Ledgers.Models.Ledger>(new GetLedgerQuery(id));

        return ledger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetLedgersQuery.Result>>> GetLedgers([FromQuery] GetLedgersQuery query)
    {
        var ledgers = await bus.InvokeAsync<IEnumerable<GetLedgersQuery.Result>>(query);

        return Ok(ledgers);
    }

    [HttpPost]
    public async Task<ActionResult<Contracts.Ledgers.Models.Ledger>> CreateLedger(CreateLedgerCommand command)
    {
        var commandResult = await bus.InvokeCommandAsync(command);

        return commandResult.Match(
            ledger => StatusCode(StatusCodes.Status201Created, ledger),
            errors => BadRequest(new { Errors = errors }));
    }

    [HttpGet("fake-error")]
    public Task<ActionResult<Contracts.Ledgers.Models.Ledger>> FakeError() => throw new DivideByZeroException("Fake error");
}
