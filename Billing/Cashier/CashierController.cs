using Microsoft.AspNetCore.Mvc;

namespace Billing.Cashier;

[ApiController]
[Route("[controller]")]
public class CashierController : ControllerBase
{
    [HttpGet]
    public IEnumerable<Models.Cashier> GetCashiers()
    {
        return [];
    }
}
