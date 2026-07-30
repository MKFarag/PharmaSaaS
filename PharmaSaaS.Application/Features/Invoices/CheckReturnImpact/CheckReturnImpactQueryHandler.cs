using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Application.Interfaces.Services;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Application.Features.Invoices.CheckReturnImpact;

public sealed class CheckReturnImpactQueryHandler(
    IUnitOfWork unitOfWork,
    IReturnItemValidatorService returnItemValidator,
    IReturnRefundCalculatorService refundCalculator) : IRequestHandler<CheckReturnImpactQuery, Result<CheckReturnImpactResponse>>
{
    public async Task<Result<CheckReturnImpactResponse>> Handle(CheckReturnImpactQuery query, CancellationToken cancellationToken)
    {
        // 1. Load invoice with its items
        if (await unitOfWork.Invoices.FindAsync(i => i.Id == query.InvoiceId, [nameof(Invoice.Items)], cancellationToken) is not { } invoice)
            return Result.Failure<CheckReturnImpactResponse>(InvoiceErrors.NotFound);

        // 2. Load prior returns
        var existingReturns = await unitOfWork.Returns.FindAllAsync(r => r.InvoiceId == invoice.Id, cancellationToken);

        // 3. Validate items in a single pass (products exist + quantities not exceeded)
        if (returnItemValidator.Validate(invoice, existingReturns, query.Request.Items) is { } validationError)
            return Result.Failure<CheckReturnImpactResponse>(validationError);

        // 4. Load customer with tier for loyalty impact assessment (null for anonymous invoices)
        Customer? customer = null;
        LoyaltyTier? currentTier = null;

        if (invoice.CustomerId.HasValue)
        {
            customer = await unitOfWork.Customers.FindAsync(
                c => c.Id == invoice.CustomerId.Value,
                [nameof(Customer.CurrentTier)],
                cancellationToken);

            currentTier = customer?.CurrentTier;
        }

        // 5. Build return item entries from invoice prices (single lookup via dictionary)
        var invoiceItemByProductId = invoice.Items.ToDictionary(i => i.ProductId);
        var returnItemEntries = query.Request.Items
            .Select(req => new ReturnItemEntry(req.ProductId, (decimal)req.Quantity, invoiceItemByProductId[req.ProductId].UnitPrice))
            .ToList();

        // 6. Calculate loyalty-aware refund (read-only — no writes, no transaction)
        var refundResult = await refundCalculator.CalculateRefundAsync(
            invoice, existingReturns, returnItemEntries, customer, cancellationToken);

        // 7. Project the customer's spend and tier status after the return
        var projectedSpend = customer is not null
            ? Math.Max(0, customer.TotalSpend - refundResult.TotalRefund)
            : 0m;

        var willDropBelowTier = currentTier is not null && projectedSpend < currentTier.SpendThreshold;

        return Result.Success(new CheckReturnImpactResponse(
            EstimatedRefund: refundResult.TotalRefund,
            DiscountClawback: refundResult.DiscountClawback,
            WillDropBelowLoyaltyTier: willDropBelowTier,
            CurrentTierName: currentTier?.Name,
            CurrentTierThreshold: currentTier?.SpendThreshold,
            ProjectedSpendAfterReturn: projectedSpend
        ));
    }
}
