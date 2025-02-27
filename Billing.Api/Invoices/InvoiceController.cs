// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Api.Invoices;

[ApiController]
[Route("[controller]")]
public class InvoiceController : ControllerBase
{
    [HttpGet]
    public IEnumerable<int> GetInvoices()
    {
        return [];
    }
}
