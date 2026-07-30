using PharmaSaaS.Domain.Entities;

namespace PharmaSaaS.Application.Interfaces.Services;

/// <summary>
/// Service responsible for calculating the correct refund amount when a customer returns
/// items from a discounted invoice, taking loyalty tier thresholds into account.
/// </summary>
public interface IReturnRefundCalculatorService
{
    /// <summary>
    /// Calculates the total refund amount for a set of return items, factoring in whether
    /// the customer's remaining purchase value still qualifies for the original loyalty discount.
    /// If the remaining total falls below the discount threshold, the discount is clawed back.
    /// Also carries a per-item breakdown so handlers can build <see cref="Return"/> entities
    /// without re-implementing the distribution math.
    /// </summary>
    /// <param name="invoice">The original invoice being returned against.</param>
    /// <param name="allExistingReturns">All returns already processed for this invoice.</param>
    /// <param name="newReturnItems">The return items being processed in this request.</param>
    /// <param name="customer">The customer associated with the invoice, or null for anonymous invoices.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="ReturnRefundResult"/> containing the total refund, discount clawback, and per-item refund amounts.</returns>
    Task<ReturnRefundResult> CalculateRefundAsync(
        Invoice invoice,
        IEnumerable<Return> allExistingReturns,
        IEnumerable<ReturnItemEntry> newReturnItems,
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
