// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Commands;
using MediatR;

namespace Billing.Api.Cashier;

[Route("[controller]")]
public class CashierController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Contracts.Cashier.Models.Cashier> GetCashiers()
    {
        return [];
    }

    [HttpPost]
    public async Task<ActionResult<Contracts.Cashier.Models.Cashier>> CreateCashier(CreateCashierCommand command)
    {
        var cashierId = await mediator.Send(command);

        return Created();
    }
}
