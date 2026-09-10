namespace PharmaSaaS.Application.Interfaces.Services;

/// <summary>
/// Unified facade for the invoice-return domain:
/// validation, refund calculation, and return-item entry building.
///
/// Replaces the former <c>IReturnItemValidatorService</c> and
/// <c>IReturnRefundCalculatorService</c> pair — both were single-method
/// interfaces used exclusively together by the two return handlers.
/// </summary>
public interface IReturnService
{
    /// <summary>
    /// Validates that every requested return item references a product on the invoice
    /// and that the quantity does not exceed what is still returnable.
    /// </summary>
    /// <param name="invoice">The invoice being returned against (must have Items loaded).</param>
    /// <param name="existingReturns">Returns already recorded for this invoice (pre-loaded from the invoice nav prop).</param>
    /// <param name="requestItems">Items the caller wants to return now.</param>
    /// <returns><c>null</c> when validation passes; the first <see cref="Error"/> violation otherwise.</returns>
    Error? ValidateReturnRequest(Invoice invoice, IEnumerable<Return> existingReturns, IEnumerable<ReturnItemRequest> requestItems);

    /// <summary>
    /// Projects <paramref name="requestItems"/> into <see cref="ReturnItemEntry"/> records
    /// by resolving each unit price from the invoice line items.
    /// </summary>
    /// <param name="invoice">The invoice being returned against (must have Items loaded).</param>
    /// <param name="requestItems">Items the caller wants to return now.</param>
    /// <returns>A list of <see cref="ReturnItemEntry"/> values ready for the refund calculator.</returns>
    IReadOnlyList<ReturnItemEntry> BuildEntries(Invoice invoice, IEnumerable<ReturnItemRequest> requestItems);

    /// <summary>
    /// Calculates the loyalty-aware refund for the given return items.
    /// Claws back the loyalty discount when the remaining invoice total drops below the qualifying tier threshold.
    /// </summary>
    /// <param name="invoice">The original invoice.</param>
    /// <param name="existingReturns">Returns already recorded for this invoice.</param>
    /// <param name="returnItemEntries">Entries built by <see cref="BuildEntries"/>.</param>
    /// <param name="customer">The customer, or <c>null</c> for anonymous invoices.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="ReturnRefundResult"/> with the total refund, clawback, and per-item amounts.</returns>
    Task<ReturnRefundResult> CalculateRefundAsync(
        Invoice invoice,
        IEnumerable<Return> existingReturns,
        IReadOnlyList<ReturnItemEntry> returnItemEntries,
        Customer? customer,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a single item in a return request with its unit price resolved from the invoice.
/// </summary>
/// <param name="ProductId">The product being returned.</param>
/// <param name="Quantity">Number of units being returned.</param>
/// <param name="UnitPrice">The price per unit at the time of the original sale.</param>
public record ReturnItemEntry(int ProductId, decimal Quantity, decimal UnitPrice);

/// <summary>The result of a refund calculation.</summary>
/// <param name="TotalRefund">The total amount to refund to the customer.</param>
/// <param name="DiscountClawback">
/// The portion of the original discount that was clawed back because the remaining purchase
/// no longer qualifies for the loyalty discount tier. Zero if the discount is still valid.
/// </param>
/// <param name="ItemRefunds">
/// Per-item refund amounts keyed by <c>ProductId</c>.
/// The handler uses these to create <see cref="Return"/> entities without repeating
/// the proportional-distribution arithmetic.
/// </param>
public record ReturnRefundResult(
    decimal TotalRefund,
    decimal DiscountClawback,
    IReadOnlyDictionary<int, decimal> ItemRefunds);

