// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.Api.Ledgers;

[ApiController]
[Route("[controller]")]
public class LedgersController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    ///     Retrieves a specific ledger by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the ledger</param>
    /// <returns>The ledger details if found</returns>
    /// <response code="200">Returns the ledger details</response>
    /// <response code="404">If the ledger is not found</response>
    /// <response code="400">If the provided ID is invalid</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Contracts.Ledgers.Models.Ledger>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    [Tags("Ledgers")]
    public async Task<ActionResult<Contracts.Ledgers.Models.Ledger>> GetLedger([FromRoute] Guid id)
    {
        var ledger = await bus.InvokeAsync<Contracts.Ledgers.Models.Ledger>(new GetLedgerQuery(id));

        return ledger;
    }

    /// <summary>
    ///     Retrieves a list of ledgers with optional filtering and pagination
    /// </summary>
    /// <param name="query">Query parameters for filtering ledgers by account type, date range, status, etc.</param>
    /// <returns>A list of ledgers matching the specified criteria</returns>
    /// <response code="200">Returns the list of ledgers</response>
    /// <response code="400">If query parameters are invalid</response>
    [HttpGet]
    [ProducesResponseType<IEnumerable<GetLedgersQuery.Result>>(StatusCodes.Status200OK)]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    [Tags("Ledgers")]
    public async Task<ActionResult<IEnumerable<GetLedgersQuery.Result>>> GetLedgers([FromQuery] GetLedgersQuery query)
    {
        var ledgers = await bus.InvokeAsync<IEnumerable<GetLedgersQuery.Result>>(query);

        return Ok(ledgers);
    }

    /// <summary>
    ///     Creates a new ledger in the accounting system
    /// </summary>
    /// <param name="command">The ledger creation request containing account details, type, and initial balance</param>
    /// <returns>The created ledger details</returns>
    /// <response code="201">Ledger created successfully</response>
    /// <response code="400">If the request data is invalid or validation fails</response>
    /// <response code="409">If a ledger with the same account number already exists</response>
    [HttpPost]
    [ProducesResponseType<Contracts.Ledgers.Models.Ledger>(StatusCodes.Status201Created)]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Tags("Ledgers")]
    public async Task<ActionResult<Contracts.Ledgers.Models.Ledger>> CreateLedger([FromBody] CreateLedgerCommand command)
    {
        var commandResult = await bus.InvokeCommandAsync(command);

        return commandResult.Match(
            ledger => StatusCode(StatusCodes.Status201Created, ledger),
            errors => BadRequest(new { Errors = errors }));
    }

    /// <summary>
    ///     Testing endpoint that throws an exception for error handling validation
    /// </summary>
    /// <returns>Never returns successfully - always throws an exception</returns>
    /// <response code="500">Always throws a DivideByZeroException for testing purposes</response>
    [HttpGet("fake-error")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Testing")]
    public Task<ActionResult<Contracts.Ledgers.Models.Ledger>> FakeError() => throw new DivideByZeroException("Fake error");
}
