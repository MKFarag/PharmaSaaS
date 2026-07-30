using PharmaSaaS.Application.Interfaces.Services;
using PharmaSaaS.Domain.Entities;

namespace PharmaSaaS.Application.Services;

/// <summary>
/// Calculates the correct refund for a return, applying loyalty discount clawback
/// when the remaining invoice total drops below the qualifying threshold.
///
/// Algorithm:
///   1. Derive the remaining invoice balance already refunded to determine how much
///      the customer is still "owed" from prior returns.
///   2. Compute the base value of items being returned now (qty × unitPrice).
///   3. Compute the remaining uninvoiced total = invoice.Total − baseReturned − baseNow.
///   4. Ask the loyalty service what discount applies to that remaining total.
///   5. Refund = FinalAmount − alreadyRefunded − (remainingTotal − newDiscount).
///   6. Distribute the total refund proportionally across items by base value.
/// </summary>
public sealed class ReturnRefundCalculatorService(
    ICustomerLoyaltyService customerLoyaltyService) : IReturnRefundCalculatorService
{
    public async Task<ReturnRefundResult> CalculateRefundAsync(
        Invoice invoice,
        IEnumerable<Return> allExistingReturns,
        IEnumerable<ReturnItemEntry> newReturnItems,
        Customer? customer,
        CancellationToken cancellationToken = default)
    {
        var returnItemsList = newReturnItems.ToList();
        var existingReturnsList = allExistingReturns.ToList();

        // How much has already been refunded for prior returns on this invoice
        var alreadyRefunded = existingReturnsList.Sum(r => r.RefundAmount);

        // Base value of items being returned now (at original selling price)
        var returnedItemsBaseValue = returnItemsList.Sum(i => i.Quantity * i.UnitPrice);

        // Base value of items returned in prior sessions
        // We reconstruct this from the invoice's discount factor applied to prior refunds.
        // Since prior refunds were calculated by the same algorithm, reversing via the factor is valid
        // only when the invoice had a uniform discount. To stay simple and correct, we track the
        // original base separately: prior base = prior refund / discountFactor (when factor > 0).
        var discountFactor = invoice.Total > 0 && invoice.FinalAmount > 0
            ? invoice.FinalAmount / invoice.Total
            : 1m;

        var previousBaseValue = discountFactor > 0
            ? alreadyRefunded / discountFactor
            : alreadyRefunded;

        // Remaining total at original prices after ALL returns (prior + current)
        var remainingTotal = Math.Max(0, invoice.Total - previousBaseValue - returnedItemsBaseValue);

        // What discount applies to the remaining total now?
        var newDiscountForRemaining = customer is not null
            ? await customerLoyaltyService.CalculateDiscountAsync(customer, remainingTotal, cancellationToken)
            : 0m;

        // What the customer should now owe for the remaining items
        var expectedFinalForRemaining = remainingTotal - newDiscountForRemaining;

        // Total refund = what was actually paid − already refunded − what customer still owes
        var totalRefund = Math.Max(0, invoice.FinalAmount - alreadyRefunded - expectedFinalForRemaining);

        // Discount clawback: the portion of the original discount lost due to falling below threshold
        var originalDiscountOnReturned = returnedItemsBaseValue * (1m - discountFactor);
        var discountClawback = Math.Max(0, invoice.DiscountAmount - newDiscountForRemaining - originalDiscountOnReturned);

        // Distribute total refund proportionally across items by their base value
        var itemRefunds = BuildItemRefunds(returnItemsList, totalRefund, returnedItemsBaseValue);

        return new ReturnRefundResult(totalRefund, discountClawback, itemRefunds);
    }

    private static IReadOnlyDictionary<int, decimal> BuildItemRefunds(
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
