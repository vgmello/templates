// Copyright (c) ABCDEG. All rights reserved.

#pragma warning disable S125

//
// public class BusinessDayEndedHandler(ILogger<BusinessDayEndedHandler> logger, IMessageBus messageBus)
// {
//     public async Task Handle(BusinessDayEnded businessDayEnded)
//     {
//         logger.LogInformation("Processing business day ended for {BusinessDate} in {Market}/{Region}",
//             businessDayEnded.BusinessDate, businessDayEnded.Market, businessDayEnded.Region);
//
//         var invoiceId = Guid.CreateVersion7();
//         var customerId = Guid.CreateVersion7();
//
//         await messageBus.PublishAsync(new InvoiceGenerated
//         {
//             InvoiceId = invoiceId,
//             InvoiceAmount = 500.75m
//         });
//
//         await messageBus.PublishAsync(new InvoiceFinalized
//         {
//             InvoiceId = invoiceId,
//             CustomerId = customerId,
//             PublicInvoiceNumber = $"INV-{businessDayEnded.BusinessDate:yyyyMMdd}-{invoiceId:N}",
//             FinalTotalAmount = 500.75m
//         });
//
//         logger.LogInformation("Generated and finalized invoice {InvoiceId} for business day {BusinessDate}",
//             invoiceId, businessDayEnded.BusinessDate);
//     }
// }
