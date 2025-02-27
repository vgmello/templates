// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Api.Cashier;

[Route("[controller]")]
public class CashierController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<Contracts.Cashier.Models.Cashier> GetCashier([FromRoute] Guid id)
    {
        var cashier = await mediator.Send(new GetCashierQuery(id));

        return cashier;
    }

    [HttpGet]
    public async Task<IEnumerable<GetCashiersQuery.Result>> GetCashiers([FromQuery] GetCashiersQuery query)
    {
        var cashiers = await mediator.Send(query);

        return cashiers;
    }

    [HttpPost]
    public async Task<ActionResult<Contracts.Cashier.Models.Cashier>> CreateCashier(CreateCashierCommand command)
    {
        var cashier = await mediator.Send(command);

        return StatusCode(StatusCodes.Status201Created, cashier);
    }
}
