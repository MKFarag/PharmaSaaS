using PharmaSaaS.Domain.Entities;

namespace PharmaSaaS.Application.Interfaces.Services;

/// <summary>
/// Service responsible for calculating loyalty discounts and updating customer loyalty points/tiers.
/// </summary>
public interface ICustomerLoyaltyService
{
    /// <summary>
    /// Calculates the discount amount for a customer based on their current loyalty tier and the invoice total.
    /// </summary>
    Task<decimal> CalculateDiscountAsync(Customer customer, decimal totalAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the customer's total spend and recalculates their loyalty tier.
    /// Use a positive amountDelta for purchases, and a negative amountDelta for returns.
    /// </summary>
    Task UpdateCustomerLoyaltyAsync(Customer customer, decimal amountDelta, CancellationToken cancellationToken = default);
}
