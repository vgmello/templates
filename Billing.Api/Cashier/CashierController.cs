using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Cashier;

[ApiController]
[Route("[controller]")]
public class CashierController : ControllerBase
{
    [HttpGet]
    public IEnumerable<Billing.Cashier.Models.Cashier> GetCashiers()
    {
        return [];
    }
}
