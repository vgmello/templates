// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Cashier;

[ApiController]
[Route("[controller]")]
public class CashierController : ControllerBase
{
    [HttpGet]
    public IEnumerable<Contracts.Cashier.Models.Cashier> GetCashiers()
    {
        return [];
    }
}
