// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Invoices;

[ApiController]
[Route("[controller]")]
public class InvoiceController : ControllerBase
{
    [HttpGet]
    public IEnumerable<Contracts.Cashier.Models.Cashier> GetInvoices()
    {
        return [];
    }
}
