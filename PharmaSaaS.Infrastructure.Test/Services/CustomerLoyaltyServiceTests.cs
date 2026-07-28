using System.Linq.Expressions;
using FakeItEasy;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Repositories;
using PharmaSaaS.Infrastructure.Services;
using Xunit;

namespace PharmaSaaS.Application.Test.Services;

public class CustomerLoyaltyServiceTests
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<LoyaltyTier> _loyaltyTierRepo = A.Fake<IGenericRepository<LoyaltyTier>>();
    private readonly IGenericRepository<DiscountRule> _discountRuleRepo = A.Fake<IGenericRepository<DiscountRule>>();
    private readonly CustomerLoyaltyService _service;

    public CustomerLoyaltyServiceTests()
    {
        A.CallTo(() => _unitOfWork.LoyaltyTiers).Returns(_loyaltyTierRepo);
        A.CallTo(() => _unitOfWork.DiscountRules).Returns(_discountRuleRepo);
        _service = new CustomerLoyaltyService(_unitOfWork);
    }

    [Fact]
    public async Task CalculateDiscountAsync_NoTier_ReturnsZero()
    {
        var customer = Customer.Create("John Doe", "123456789");
        // CurrentTierId is null

        var discount = await _service.CalculateDiscountAsync(customer, 1000m);

        Assert.Equal(0m, discount);
        A.CallTo(() => _discountRuleRepo.FindAllAsync(A<Expression<Func<DiscountRule, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task CalculateDiscountAsync_NoRulesForTier_ReturnsZero()
    {
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(100, 1);

        A.CallTo(() => _discountRuleRepo.FindAllAsync(A<Expression<Func<DiscountRule, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new List<DiscountRule>());

        var discount = await _service.CalculateDiscountAsync(customer, 1000m);

        Assert.Equal(0m, discount);
    }

    [Fact]
    public async Task CalculateDiscountAsync_InvoiceAmountBelowMin_ReturnsZero()
    {
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(100, 1);

        var rule = DiscountRule.Create(1, 1000m, 10m);
        A.CallTo(() => _discountRuleRepo.FindAllAsync(A<Expression<Func<DiscountRule, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new List<DiscountRule> { rule });

        var discount = await _service.CalculateDiscountAsync(customer, 500m);

        Assert.Equal(0m, discount);
    }

    [Fact]
    public async Task CalculateDiscountAsync_InvoiceAmountMatchesRule_ReturnsCalculatedDiscount()
    {
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(100, 1);

        var rule = DiscountRule.Create(1, 1000m, 10m); // 10%
        A.CallTo(() => _discountRuleRepo.FindAllAsync(A<Expression<Func<DiscountRule, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new List<DiscountRule> { rule });

        var discount = await _service.CalculateDiscountAsync(customer, 2000m);

        Assert.Equal(200m, discount); // 10% of 2000
    }

    [Fact]
    public async Task CalculateDiscountAsync_MultipleRules_PicksHighestApplicable()
    {
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(100, 1);

        var rule1 = DiscountRule.Create(1, 500m, 5m);   // 5%
        var rule2 = DiscountRule.Create(1, 1000m, 10m); // 10%
        var rule3 = DiscountRule.Create(1, 2000m, 20m); // 20%
        A.CallTo(() => _discountRuleRepo.FindAllAsync(A<Expression<Func<DiscountRule, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new List<DiscountRule> { rule1, rule2, rule3 });

        var discount = await _service.CalculateDiscountAsync(customer, 1500m);

        // Should pick rule2 because 1500 >= 1000 and < 2000
        Assert.Equal(150m, discount); // 10% of 1500
    }

    [Fact]
    public async Task UpdateCustomerLoyaltyAsync_PurchaseNoTierChange_UpdatesSpend()
    {
        var customer = Customer.Create("John Doe", "123456789");
        // Tiers: Bronze(100), Silver(500)
        var bronze = LoyaltyTier.Create("Bronze", 100m);
        bronze.Id = 1;
        var silver = LoyaltyTier.Create("Silver", 500m);
        silver.Id = 2;

        A.CallTo(() => _loyaltyTierRepo.GetAllAsync(A<string[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new List<LoyaltyTier> { bronze, silver });

        await _service.UpdateCustomerLoyaltyAsync(customer, 50m);

        Assert.Equal(50m, customer.TotalSpend);
        Assert.Null(customer.CurrentTierId); // 50 < 100
    }

    [Fact]
    public async Task UpdateCustomerLoyaltyAsync_PurchaseTierUpgrades_UpdatesSpendAndTier()
    {
        var customer = Customer.Create("John Doe", "123456789");
        var bronze = LoyaltyTier.Create("Bronze", 100m);
        bronze.Id = 1;
        var silver = LoyaltyTier.Create("Silver", 500m);
        silver.Id = 2;

        A.CallTo(() => _loyaltyTierRepo.GetAllAsync(A<string[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new List<LoyaltyTier> { bronze, silver });

        await _service.UpdateCustomerLoyaltyAsync(customer, 150m);

        Assert.Equal(150m, customer.TotalSpend);
        Assert.Equal(1, customer.CurrentTierId); // Bronze

        await _service.UpdateCustomerLoyaltyAsync(customer, 400m);

        Assert.Equal(550m, customer.TotalSpend);
        Assert.Equal(2, customer.CurrentTierId); // Silver
    }

    [Fact]
    public async Task UpdateCustomerLoyaltyAsync_ReturnTierDowngrades_UpdatesSpendAndTier()
    {
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(600m, 2);

        var bronze = LoyaltyTier.Create("Bronze", 100m);
        bronze.Id = 1;
        var silver = LoyaltyTier.Create("Silver", 500m);
        silver.Id = 2;

        A.CallTo(() => _loyaltyTierRepo.GetAllAsync(A<string[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new List<LoyaltyTier> { bronze, silver });

        // Return $200 worth of products
        await _service.UpdateCustomerLoyaltyAsync(customer, -200m);

        Assert.Equal(400m, customer.TotalSpend);
        Assert.Equal(1, customer.CurrentTierId); // Downgraded to Bronze
    }

    [Fact]
    public async Task UpdateCustomerLoyaltyAsync_ReturnBelowZero_SpendCappedAtZero()
    {
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(50m, null);

        var bronze = LoyaltyTier.Create("Bronze", 100m);
        bronze.Id = 1;

        A.CallTo(() => _loyaltyTierRepo.GetAllAsync(A<string[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(new List<LoyaltyTier> { bronze });

        await _service.UpdateCustomerLoyaltyAsync(customer, -100m);

        Assert.Equal(0m, customer.TotalSpend); // Capped at 0
        Assert.Null(customer.CurrentTierId);
    }
}
