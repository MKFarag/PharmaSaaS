using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Application.Interfaces.Services;
using PharmaSaaS.Domain.Entities;

namespace PharmaSaaS.Application.Services;

public sealed class CustomerLoyaltyService(IUnitOfWork unitOfWork) : ICustomerLoyaltyService
{
    public async Task<decimal> CalculateDiscountAsync(Customer customer, decimal totalAmount, CancellationToken cancellationToken = default)
    {
        if (!customer.CurrentTierId.HasValue)
            return 0;

        var rules = await unitOfWork.DiscountRules.FindAllAsync(r => r.LoyaltyTierId == customer.CurrentTierId.Value, cancellationToken);

        var applicableRule = rules
            .Where(r => r.MinInvoiceAmount <= totalAmount)
            .OrderByDescending(r => r.MinInvoiceAmount)
            .FirstOrDefault();

        if (applicableRule is not null)
            return totalAmount * (applicableRule.DiscountPercent / 100m);

        return 0;
    }

    public async Task UpdateCustomerLoyaltyAsync(Customer customer, decimal amountDelta, CancellationToken cancellationToken = default)
    {
        var newSpend = Math.Max(0, customer.TotalSpend + amountDelta);

        var allTiers = await unitOfWork.LoyaltyTiers.GetAllAsync(cancellationToken);

        var newTier = allTiers
            .Where(t => t.SpendThreshold <= newSpend)
            .OrderByDescending(t => t.SpendThreshold)
            .FirstOrDefault();

        customer.UpdateSpendAndTier(newSpend, newTier?.Id);
    }
}
