// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Api.Invoices;

[ApiController]
[Route("[controller]")]
public class InvoicesController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<int>> GetInvoices()
    {
        int[] result = [1, 2, 3];

        return Ok(result);
    }
}
