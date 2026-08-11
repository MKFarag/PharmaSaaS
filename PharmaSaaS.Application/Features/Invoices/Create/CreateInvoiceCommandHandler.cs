using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Application.Interfaces.Services;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Application.Features.Invoices.Create;

public sealed class CreateInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    ICustomerLoyaltyService customerLoyaltyService) : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDetailResponse>>
{
    public async Task<Result<InvoiceDetailResponse>> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        // 1. Load active shift
        if (await unitOfWork.Shifts.TrackedFindAsync(s => s.Status == ShiftStatus.Open, cancellationToken) is not { } shift)
            return Result.Failure<InvoiceDetailResponse>(InvoiceErrors.ShiftNotOpen);

        var productIds = command.Request.Items.Select(x => x.ProductId).ToList();

        // 2. Load tracked inventories (with Product for price)
        var inventories = (await unitOfWork.Inventories
            .TrackedFindAllAsync(inv => productIds.Contains(inv.ProductId), [nameof(Inventory.Product)], cancellationToken))
            .ToList();

        if (inventories.Count != productIds.Count)
            return Result.Failure<InvoiceDetailResponse>(InvoiceErrors.ProductNotFound);

        // 3. Build a lookup and validate stock in a single pass, accumulating total
        var inventoryByProductId = inventories.ToDictionary(inv => inv.ProductId);
        decimal total = 0;

        foreach (var item in command.Request.Items)
        {
            var inventory = inventoryByProductId[item.ProductId];

            if (inventory.Quantity < item.Quantity)
                return Result.Failure<InvoiceDetailResponse>(InvoiceErrors.InsufficientStock);

            total += (decimal)item.Quantity * inventory.SellingPrice;
        }

        // 4. Calculate loyalty discount (if invoice is for a known customer)
        decimal discountAmount = 0;
        Customer? customer = null;

        if (command.Request.CustomerId.HasValue)
        {
            customer = await unitOfWork.Customers.GetAsync([command.Request.CustomerId.Value], cancellationToken);

            if (customer is not null)
                discountAmount = await customerLoyaltyService.CalculateDiscountAsync(customer, total, cancellationToken);
        }

        // 5. Begin transaction — all writes below are atomic
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 6. Persist the invoice header to get its generated ID
            var invoice = Invoice.Create(shift.Id, customer?.Id, total, discountAmount, command.Request.Items.Count);
            await unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            // 7. Create invoice line items and deduct stock — reuse the dictionary built above
            var invoiceItems = command.Request.Items
                .Select(item =>
                {
                    var inventory = inventoryByProductId[item.ProductId];
                    inventory.DeductStock(item.Quantity);
                    inventory.SetLastSold();
                    inventory.AddToSoldUnits(item.Quantity);
                    return InvoiceItem.Create(invoice.Id, item.ProductId, item.Quantity, inventory.SellingPrice);
                })
                .ToList();

            await unitOfWork.InvoiceItems.AddRangeAsync(invoiceItems, cancellationToken);

            // 8. Update shift and customer loyalty
            shift.AddSale(invoice.FinalAmount);

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
