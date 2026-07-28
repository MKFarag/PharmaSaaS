using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

using PharmaSaaS.Application.Interfaces.Services;

namespace PharmaSaaS.Application.Features.Invoices.Create;

public sealed class CreateInvoiceCommandHandler(IUnitOfWork unitOfWork, ICustomerLoyaltyService customerLoyaltyService) : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDetailResponse>>
{
    public async Task<Result<InvoiceDetailResponse>> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        // 1. Load active Shift
        if (await unitOfWork.Shifts.TrackedFindAsync(s => s.Status == ShiftStatus.Open, cancellationToken) is not { } shift)
            return Result.Failure<InvoiceDetailResponse>(InvoiceErrors.ShiftNotOpen);

        var productIds = command.Request.Items.Select(x => x.ProductId).ToList();

        // 3. Load tracked Inventories
        var inventories = (await unitOfWork.Inventories
            .TrackedFindAllAsync(inv => productIds.Contains(inv.ProductId), [nameof(Inventory.Product)], cancellationToken)).ToList();

        if (inventories.Count != productIds.Count)
            return Result.Failure<InvoiceDetailResponse>(InvoiceErrors.ProductNotFound);

        // 4. Calculate total and validate stock
        decimal total = 0;
        foreach (var item in command.Request.Items)
        {
            var inventory = inventories.First(x => x.ProductId == item.ProductId);

            if (inventory.Quantity < item.Quantity)
                return Result.Failure<InvoiceDetailResponse>(InvoiceErrors.InsufficientStock);

            total += (decimal)item.Quantity * inventory.SellingPrice;
        }

        // 5. Calculate discount if Customer
        decimal discountAmount = 0;
        Customer? customer = null;

        if (command.Request.CustomerId.HasValue)
        {
            customer = await unitOfWork.Customers.GetAsync([command.Request.CustomerId.Value], cancellationToken);

            if (customer is not null)
                discountAmount = await customerLoyaltyService.CalculateDiscountAsync(customer, total, cancellationToken);
        }

        // 6. Begin transaction
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 7. Create Invoice
            var invoice = Invoice.Create(shift.Id, customer?.Id, total, discountAmount, command.Request.Items.Count);
            await unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken); // Get Invoice.Id

            // 8. Create InvoiceItems and deduct stock
            var invoiceItems = new List<InvoiceItem>();
            foreach (var item in command.Request.Items)
            {
                var inventory = inventories.First(x => x.ProductId == item.ProductId);
                
                var invoiceItem = InvoiceItem.Create(invoice.Id, item.ProductId, item.Quantity, inventory.SellingPrice);
                invoiceItems.Add(invoiceItem);

                inventory.DeductStock(item.Quantity);
            }

            await unitOfWork.InvoiceItems.AddRangeAsync(invoiceItems, cancellationToken);

            // 9. Update Shift
            shift.AddSale(invoice.FinalAmount);

            // 10. Update Customer Spend & Tier
            if (customer is not null)
                await customerLoyaltyService.UpdateCustomerLoyaltyAsync(customer, invoice.FinalAmount, cancellationToken);

            await unitOfWork.CompleteAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var savedInvoice = await unitOfWork.Invoices.FindAsync(i => i.Id == invoice.Id, Invoice.DetailIncludes, cancellationToken);

            return Result.Success(savedInvoice!.Adapt<InvoiceDetailResponse>());
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
