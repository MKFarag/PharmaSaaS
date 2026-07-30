using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Application.Interfaces.Services;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Application.Features.Invoices.ProcessReturn;

public sealed class ProcessReturnCommandHandler(
    IUnitOfWork unitOfWork,
    IReturnItemValidatorService returnItemValidator,
    IReturnRefundCalculatorService refundCalculator,
    ICustomerLoyaltyService customerLoyaltyService) : IRequestHandler<ProcessReturnCommand, Result<ProcessReturnResponse>>
{
    public async Task<Result<ProcessReturnResponse>> Handle(ProcessReturnCommand command, CancellationToken cancellationToken)
    {
        // 1. Load invoice with its items
        if (await unitOfWork.Invoices.TrackedFindAsync(i => i.Id == command.InvoiceId, [nameof(Invoice.Items)], cancellationToken) is not { } invoice)
            return Result.Failure<ProcessReturnResponse>(InvoiceErrors.NotFound);

        // 2. Load prior returns
        var existingReturns = await unitOfWork.Returns.FindAllAsync(r => r.InvoiceId == invoice.Id, cancellationToken);

        // 3. Validate items in a single pass (products exist + quantities not exceeded)
        if (returnItemValidator.Validate(invoice, existingReturns, command.Request.Items) is { } validationError)
            return Result.Failure<ProcessReturnResponse>(validationError);

        // 4. Load active shift
        if (await unitOfWork.Shifts.TrackedFindAsync(s => s.Status == ShiftStatus.Open, cancellationToken) is not { } currentShift)
            return Result.Failure<ProcessReturnResponse>(InvoiceErrors.ShiftNotOpen);

        // 5. Load customer for loyalty-aware refund calculation
        Customer? customer = null;
        if (invoice.CustomerId.HasValue)
            customer = await unitOfWork.Customers.GetAsync([invoice.CustomerId.Value], cancellationToken);

        // 6. Build return item entries from invoice prices (single lookup via dictionary)
        var invoiceItemByProductId = invoice.Items.ToDictionary(i => i.ProductId);
        var returnItemEntries = command.Request.Items
            .Select(req => new ReturnItemEntry(req.ProductId, (decimal)req.Quantity, invoiceItemByProductId[req.ProductId].UnitPrice))
            .ToList();

        // 7. Calculate loyalty-aware refund (claws back discount if remaining total drops below threshold)
        var refundResult = await refundCalculator.CalculateRefundAsync(
            invoice, existingReturns, returnItemEntries, customer, cancellationToken);

        // 8. Build Return entities using per-item amounts provided by the calculator
        var newReturns = returnItemEntries
            .Select(entry => Return.Create(invoice.Id, entry.ProductId, (int)entry.Quantity, refundResult.ItemRefunds[entry.ProductId]))
            .ToList();

        // 9. Begin transaction — all writes below are atomic
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await unitOfWork.Returns.AddRangeAsync(newReturns, cancellationToken);

            // Restore inventory
            var productIds = command.Request.Items.Select(x => x.ProductId).ToList();
            var inventories = await unitOfWork.Inventories.TrackedFindAllAsync(inv => productIds.Contains(inv.ProductId), cancellationToken);

            foreach (var req in command.Request.Items)
                inventories.FirstOrDefault(i => i.ProductId == req.ProductId)?.AddStock((int)req.Quantity);

            currentShift.AddReturn(refundResult.TotalRefund);
            invoice.MarkHasReturn();

            if (customer is not null)
                await customerLoyaltyService.UpdateCustomerLoyaltyAsync(customer, -refundResult.TotalRefund, cancellationToken);

            await unitOfWork.CompleteAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(new ProcessReturnResponse(refundResult.TotalRefund, refundResult.DiscountClawback));
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
