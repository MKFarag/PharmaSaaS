namespace PharmaSaaS.Application.Services;

/// <summary>
/// Implements the full invoice-return pipeline:
///   1. ValidateReturnRequest — checks product membership and returnable quantities.
///   2. BuildEntries — resolves unit prices from the loaded invoice items.
///   3. CalculateRefundAsync — computes loyalty-aware refund with optional discount clawback.
///
/// Algorithm for CalculateRefundAsync:
///   1. Reconstruct the base value already refunded (prior sessions) via the invoice discount factor.
///   2. Compute the base value of items being returned now (qty × unitPrice).
///   3. Remaining total at original prices = invoice.Total − priorBase − currentBase.
///   4. Ask the loyalty service what discount applies to that remaining total.
///   5. TotalRefund = FinalAmount − alreadyRefunded − (remainingTotal − newDiscount).
///   6. Distribute the total refund proportionally across items by base value.
/// </summary>
public sealed class ReturnService(ICustomerLoyaltyService customerLoyaltyService) : IReturnService
{
    public Error? ValidateReturnRequest(Invoice invoice, IEnumerable<Return> existingReturns, IEnumerable<ReturnItemRequest> requestItems)
    {
        var returnedByProduct = existingReturns
            .GroupBy(r => r.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

        foreach (var reqItem in requestItems)
        {
            if (invoice.Items.FirstOrDefault(i => i.ProductId == reqItem.ProductId) is not { } originalItem)
                return InvoiceErrors.ProductNotInInvoice;

            var alreadyReturned = returnedByProduct.GetValueOrDefault(reqItem.ProductId, 0);

            if (alreadyReturned + reqItem.Quantity > originalItem.Quantity)
                return InvoiceErrors.ReturnQuantityExceeded;
        }

        return null;
    }

    public IReadOnlyList<ReturnItemEntry> BuildEntries(Invoice invoice, IEnumerable<ReturnItemRequest> requestItems)
    {
        var invoiceItemByProductId = invoice.Items.ToDictionary(i => i.ProductId);

        return [.. requestItems
            .Select(req => new ReturnItemEntry
            (
                req.ProductId,
                (decimal)req.Quantity,
                invoiceItemByProductId[req.ProductId].UnitPrice
            ))];
    }

    public async Task<ReturnRefundResult> CalculateRefundAsync(
        Invoice invoice,
        IEnumerable<Return> existingReturns,
        IReadOnlyList<ReturnItemEntry> returnItemEntries,
        Customer? customer,
        CancellationToken cancellationToken = default)
    {
        var existingReturnsList = existingReturns.ToList();

        var alreadyRefunded = existingReturnsList.Sum(r => r.RefundAmount);

        var returnedItemsBaseValue = returnItemEntries.Sum(i => i.Quantity * i.UnitPrice);

        var discountFactor = invoice.Total > 0 && invoice.FinalAmount > 0
            ? invoice.FinalAmount / invoice.Total
            : 1m;

        var previousBaseValue = discountFactor > 0
            ? alreadyRefunded / discountFactor
            : alreadyRefunded;

        var remainingTotal = Math.Max(0, invoice.Total - previousBaseValue - returnedItemsBaseValue);

        var newDiscountForRemaining = customer is not null
            ? await customerLoyaltyService.CalculateDiscountAsync(customer, remainingTotal, cancellationToken)
            : 0m;

        var expectedFinalForRemaining = remainingTotal - newDiscountForRemaining;
        var totalRefund = Math.Max(0, invoice.FinalAmount - alreadyRefunded - expectedFinalForRemaining);

        var originalDiscountOnReturned = returnedItemsBaseValue * (1m - discountFactor);
        var discountClawback = Math.Max(0, invoice.DiscountAmount - newDiscountForRemaining - originalDiscountOnReturned);

        var itemRefunds = BuildItemRefunds(returnItemEntries, totalRefund, returnedItemsBaseValue);

        return new ReturnRefundResult(totalRefund, discountClawback, itemRefunds);
    }

    private static Dictionary<int, decimal> BuildItemRefunds(
        IReadOnlyList<ReturnItemEntry> items,
        decimal totalRefund,
        decimal totalBaseValue)
    {
        if (totalBaseValue == 0)
            return items.ToDictionary(i => i.ProductId, _ => 0m);

        return items.ToDictionary(
            i => i.ProductId,
            i => Math.Round(i.Quantity * i.UnitPrice / totalBaseValue * totalRefund, 4));
    }
}
