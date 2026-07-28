using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

using PharmaSaaS.Application.Interfaces.Services;

namespace PharmaSaaS.Application.Features.Invoices.ProcessReturn;

public sealed class ProcessReturnCommandHandler(IUnitOfWork unitOfWork, ICustomerLoyaltyService customerLoyaltyService) : IRequestHandler<ProcessReturnCommand, Result<ProcessReturnResponse>>
{
    public async Task<Result<ProcessReturnResponse>> Handle(ProcessReturnCommand command, CancellationToken cancellationToken)
    {
        // 1. Load Invoice with Items
        if (await unitOfWork.Invoices.TrackedFindAsync(i => i.Id == command.InvoiceId, [nameof(Invoice.Items)], cancellationToken) is not { } invoice)
            return Result.Failure<ProcessReturnResponse>(InvoiceErrors.NotFound);

        // 2. Validate return items
        foreach (var reqItem in command.Request.Items)
            if (invoice.Items.FirstOrDefault(i => i.ProductId == reqItem.ProductId) is null)
                return Result.Failure<ProcessReturnResponse>(InvoiceErrors.ProductNotInInvoice);

        // Get currently returned quantities
        var existingReturns = await unitOfWork.ReturnInvoiceItems.FindAllAsync(
            ri => ri.ReturnInvoice.OriginalInvoiceId == invoice.Id,
            cancellationToken);

        decimal totalRefund = 0;
        decimal discountFactor = invoice.Total > 0 ? invoice.FinalAmount / invoice.Total : 1m;
        var newReturnItems = new List<ReturnInvoiceItem>();

        foreach (var reqItem in command.Request.Items)
        {
            var originalItem = invoice.Items.First(i => i.ProductId == reqItem.ProductId);
            var alreadyReturned = existingReturns.Where(ri => ri.ProductId == reqItem.ProductId).Sum(ri => ri.Quantity);

            if (alreadyReturned + reqItem.Quantity > originalItem.Quantity)
                return Result.Failure<ProcessReturnResponse>(InvoiceErrors.ReturnQuantityExceeded);

            var baseRefundAmount = (decimal)reqItem.Quantity * originalItem.UnitPrice;
            totalRefund += baseRefundAmount * discountFactor;
            
            // We will create the entity later
        }

        // 3. Load active shift to register return
        if (await unitOfWork.Shifts.TrackedFindAsync(s => s.Status == ShiftStatus.Open, cancellationToken) is not { } currentShift)
            return Result.Failure<ProcessReturnResponse>(InvoiceErrors.ShiftNotOpen);

        // 4. Begin Transaction
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create ReturnInvoice
            var returnInvoice = ReturnInvoice.Create(invoice.Id, currentShift.Id, totalRefund);
            await unitOfWork.ReturnInvoices.AddAsync(returnInvoice, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            foreach (var reqItem in command.Request.Items)
            {
                var originalItem = invoice.Items.First(i => i.ProductId == reqItem.ProductId);  
                var discountedUnitPrice = originalItem.UnitPrice * discountFactor;
                var returnItem = ReturnInvoiceItem.Create(returnInvoice.Id, reqItem.ProductId, reqItem.Quantity, discountedUnitPrice);
                newReturnItems.Add(returnItem);
            }

            await unitOfWork.ReturnInvoiceItems.AddRangeAsync(newReturnItems, cancellationToken);

            // Restore Inventory
            var productIds = command.Request.Items.Select(x => x.ProductId).ToList();
            var inventories = await unitOfWork.Inventories.TrackedFindAllAsync(inv => productIds.Contains(inv.ProductId), cancellationToken);

            foreach (var reqItem in command.Request.Items)
            {
                var inv = inventories.FirstOrDefault(i => i.ProductId == reqItem.ProductId);
                inv?.AddStock(reqItem.Quantity);
            }

            // Update Shift
            currentShift.AddReturn(totalRefund);

            // Update Invoice
            invoice.MarkHasReturn();

            // Update Customer Spend
            if (invoice.CustomerId.HasValue)
            {
                var customer = await unitOfWork.Customers.GetAsync([invoice.CustomerId.Value], cancellationToken);

                if (customer is not null)
                    await customerLoyaltyService.UpdateCustomerLoyaltyAsync(customer, -totalRefund, cancellationToken);
            }

            await unitOfWork.CompleteAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(new ProcessReturnResponse(returnInvoice.Id, totalRefund));
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
