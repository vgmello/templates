// Copyright (c) ABCDEG. All rights reserved.

using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Cashier;

[ApiController]
[Route("[controller]")]
public class CashierController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Contracts.Cashier.Models.Cashier> GetCashiers()
    {
        return [];
    }

    [HttpPost()]
    public Task<ActionResult<Contracts.Cashier.Models.Cashier>> CreateCashier()
    {
        return mediator.Send(new CreateCashierDbCommand());
    }
}
