using FakeItEasy;

using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Application.Services;
using PharmaSaaS.Domain.Entities;
using System.Linq.Expressions;
using Xunit;

namespace PharmaSaaS.Application.Test.Services;

public class CustomerLoyaltyServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CustomerLoyaltyService _sut;

    public CustomerLoyaltyServiceTests()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _sut = new CustomerLoyaltyService(_unitOfWork);
    }

    #region CalculateDiscountAsync

    [Fact]
    public async Task CalculateDiscountAsync_WhenCustomerHasNoTier_ReturnsZero()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "123456789");
        // customer.CurrentTierId is null by default

        // Act
        var result = await _sut.CalculateDiscountAsync(customer, 1000m);

        // Assert
        Assert.Equal(0m, result);
        A.CallTo(() => _unitOfWork.DiscountRules.FindAllAsync(A<Expression<Func<DiscountRule, bool>>>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CalculateDiscountAsync_WhenNoRulesMatch_ReturnsZero()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(5000m, 1); // Has TierId = 1

        var rules = new List<DiscountRule>
        {
            DiscountRule.Create(1, 2000m, 10m) // Min amount is 2000
        };

        A.CallTo(() => _unitOfWork.DiscountRules.FindAllAsync(A<Expression<Func<DiscountRule, bool>>>._, A<CancellationToken>._))
            .Returns(rules);

        // Act
        var result = await _sut.CalculateDiscountAsync(customer, 1000m); // Total is 1000, less than 2000

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WhenOneRuleMatches_ReturnsCalculatedDiscount()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(5000m, 1);

        var rules = new List<DiscountRule>
        {
            DiscountRule.Create(1, 500m, 10m) // 10% discount for >= 500
        };

        A.CallTo(() => _unitOfWork.DiscountRules.FindAllAsync(A<Expression<Func<DiscountRule, bool>>>._, A<CancellationToken>._))
            .Returns(rules);

        // Act
        var result = await _sut.CalculateDiscountAsync(customer, 1000m);

        // Assert
        Assert.Equal(100m, result); // 10% of 1000
    }

    [Fact]
    public async Task CalculateDiscountAsync_WhenMultipleRulesMatch_PicksRuleWithHighestMinAmount()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(5000m, 1);

        var rules = new List<DiscountRule>
        {
            DiscountRule.Create(1, 500m, 10m),  // 10% for >= 500
            DiscountRule.Create(1, 1000m, 15m), // 15% for >= 1000
            DiscountRule.Create(1, 2000m, 20m)  // 20% for >= 2000
        };

        A.CallTo(() => _unitOfWork.DiscountRules.FindAllAsync(A<Expression<Func<DiscountRule, bool>>>._, A<CancellationToken>._))
            .Returns(rules);

        // Act
        var result = await _sut.CalculateDiscountAsync(customer, 1500m); // Should pick the 1000m rule (15%)

        // Assert
        Assert.Equal(225m, result); // 15% of 1500
    }

    #endregion

    #region UpdateCustomerLoyaltyAsync

    [Fact]
    public async Task UpdateCustomerLoyaltyAsync_WhenAmountPositive_IncreasesSpendAndAssignsTier()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "123456789");
        // Initial spend is 0

        var silver = LoyaltyTier.Create("Silver", 1000m);
        silver.Id = 1;
        var gold = LoyaltyTier.Create("Gold", 5000m);
        gold.Id = 2;
        var tiers = new List<LoyaltyTier> { silver, gold };

        A.CallTo(() => _unitOfWork.LoyaltyTiers.GetAllAsync(A<CancellationToken>._))
            .Returns(tiers);

        // Act
        await _sut.UpdateCustomerLoyaltyAsync(customer, 2000m); // New spend will be 2000

        // Assert
        Assert.Equal(2000m, customer.TotalSpend);
        Assert.Equal(1, customer.CurrentTierId); // Should match Silver (>= 1000 but < 5000)
    }

    [Fact]
    public async Task UpdateCustomerLoyaltyAsync_WhenAmountNegative_DecreasesSpendAndDowngradesTier()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(6000m, 2); // Gold tier

        var silver = LoyaltyTier.Create("Silver", 1000m);
        silver.Id = 1;
        var gold = LoyaltyTier.Create("Gold", 5000m);
        gold.Id = 2;
        var tiers = new List<LoyaltyTier> { silver, gold };

        A.CallTo(() => _unitOfWork.LoyaltyTiers.GetAllAsync(A<CancellationToken>._))
            .Returns(tiers);

        // Act
        await _sut.UpdateCustomerLoyaltyAsync(customer, -4000m); // Return of 4000 -> New spend is 2000

        // Assert
        Assert.Equal(2000m, customer.TotalSpend);
        Assert.Equal(1, customer.CurrentTierId); // Downgraded to Silver
    }

    [Fact]
    public async Task UpdateCustomerLoyaltyAsync_WhenAmountLargelyNegative_ClampsToZeroAndRemovesTier()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "123456789");
        customer.UpdateSpendAndTier(1500m, 1);

        var silver = LoyaltyTier.Create("Silver", 1000m);
        silver.Id = 1;
        var tiers = new List<LoyaltyTier> { silver };

        A.CallTo(() => _unitOfWork.LoyaltyTiers.GetAllAsync(A<CancellationToken>._))
            .Returns(tiers);

        // Act
        await _sut.UpdateCustomerLoyaltyAsync(customer, -5000m); // New spend conceptually -3500, but should clamp to 0

        // Assert
        Assert.Equal(0m, customer.TotalSpend);
        Assert.Null(customer.CurrentTierId); // No tier matches spend 0
    }

    [Fact]
    public async Task UpdateCustomerLoyaltyAsync_WhenNoTiersExist_SetsTierToNull()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "123456789");
        
        A.CallTo(() => _unitOfWork.LoyaltyTiers.GetAllAsync(A<CancellationToken>._))
            .Returns(new List<LoyaltyTier>()); // Empty tiers

        // Act
        await _sut.UpdateCustomerLoyaltyAsync(customer, 5000m);

        // Assert
        Assert.Equal(5000m, customer.TotalSpend);
        Assert.Null(customer.CurrentTierId);
    }

    #endregion
}
