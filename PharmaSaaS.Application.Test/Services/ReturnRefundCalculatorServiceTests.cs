using FakeItEasy;
using PharmaSaaS.Application.Interfaces.Services;
using PharmaSaaS.Application.Services;
using PharmaSaaS.Domain.Entities;
using Xunit;

namespace PharmaSaaS.Application.Test.Services;

public class ReturnRefundCalculatorServiceTests
{
    private readonly ICustomerLoyaltyService _loyaltyService;
    private readonly ReturnRefundCalculatorService _sut;

    public ReturnRefundCalculatorServiceTests()
    {
        _loyaltyService = A.Fake<ICustomerLoyaltyService>();
        _sut = new ReturnRefundCalculatorService(_loyaltyService);
    }

    [Fact]
    public async Task CalculateRefundAsync_WhenNoCustomer_ReturnsFullBaseRefundWithNoClawback()
    {
        // Arrange — anonymous invoice, no discount
        // Invoice: 200 total, 0 discount, paid 200
        var invoice = Invoice.Create(1, null, 200m, 0m, 2);
        var returnItems = new[] { new ReturnItemEntry(1, 2, 50m) }; // 2 × 50 = 100 base

        // Act
        var result = await _sut.CalculateRefundAsync(invoice, [], returnItems, null);

        // Assert
        Assert.Equal(100m, result.TotalRefund);
        Assert.Equal(0m, result.DiscountClawback);
        Assert.Equal(100m, result.ItemRefunds[1]); // all refund goes to single item
        A.CallTo(() => _loyaltyService.CalculateDiscountAsync(A<Customer>._, A<decimal>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CalculateRefundAsync_WhenCustomerStillAboveThreshold_RetainsDiscount()
    {
        // Arrange
        // Invoice: 1000 total, 100 discount (10%), paid 900
        // Returning 1 item × 100 → remaining 900 → still qualifies → new discount = 90
        // Refund = 900 (paid) − 0 (prior) − (900 − 90) = 90
        var invoice = Invoice.Create(1, 1, 1000m, 100m, 2);
        var customer = Customer.Create("John", "123");
        var returnItems = new[] { new ReturnItemEntry(1, 1, 100m) };

        A.CallTo(() => _loyaltyService.CalculateDiscountAsync(customer, 900m, A<CancellationToken>._))
            .Returns(90m);

        // Act
        var result = await _sut.CalculateRefundAsync(invoice, [], returnItems, customer);

        // Assert
        Assert.Equal(90m, result.TotalRefund);
        Assert.Equal(0m, result.DiscountClawback);
        Assert.True(result.ItemRefunds.ContainsKey(1));
    }

    [Fact]
    public async Task CalculateRefundAsync_WhenCustomerDropsBelowThreshold_ClawsBackDiscount()
    {
        // Arrange
        // Invoice: 500 total, 50 discount (10%), paid 450. Threshold = 500.
        // Returning item worth 200 → remaining 300 → no longer qualifies → discount = 0
        // Refund = 450 − 0 − (300 − 0) = 150
        var invoice = Invoice.Create(1, 1, 500m, 50m, 5);
        var customer = Customer.Create("Jane", "456");
        var returnItems = new[] { new ReturnItemEntry(1, 1, 200m) };

        A.CallTo(() => _loyaltyService.CalculateDiscountAsync(customer, 300m, A<CancellationToken>._))
            .Returns(0m);

        // Act
        var result = await _sut.CalculateRefundAsync(invoice, [], returnItems, customer);

        // Assert
        Assert.Equal(150m, result.TotalRefund);
        Assert.True(result.DiscountClawback > 0m);
    }

    [Fact]
    public async Task CalculateRefundAsync_WhenFullInvoiceReturned_RefundsEntireAmountPaid()
    {
        // Arrange — customer returns ALL items from a zero-discount invoice
        var invoice = Invoice.Create(1, 1, 300m, 0m, 3);
        var customer = Customer.Create("Mira", "111");
        var returnItems = new[]
        {
            new ReturnItemEntry(1, 1, 100m),
            new ReturnItemEntry(2, 1, 100m),
            new ReturnItemEntry(3, 1, 100m)
        };

        A.CallTo(() => _loyaltyService.CalculateDiscountAsync(customer, 0m, A<CancellationToken>._))
            .Returns(0m);

        // Act
        var result = await _sut.CalculateRefundAsync(invoice, [], returnItems, customer);

        // Assert — entire 300 refunded, proportionally split across 3 items
        Assert.Equal(300m, result.TotalRefund);
        Assert.Equal(0m, result.DiscountClawback);
        Assert.Equal(3, result.ItemRefunds.Count);
    }

    [Fact]
    public async Task CalculateRefundAsync_WhenPriorReturnsExist_AccountsForAlreadyRefunded()
    {
        // Arrange
        // Invoice: 600 total, 60 discount (10%), paid 540. Factor = 0.9.
        // Prior return: product 2, refundAmount = 180 (which is 200 base × 0.9).
        // Now returning product 1 worth 200 base.
        // priorBase = 180 / 0.9 = 200. remaining = 600 − 200 − 200 = 200. discount(200) = 0.
        // Refund = 540 − 180 − (200 − 0) = 160.
        var invoice = Invoice.Create(1, 1, 600m, 60m, 3);
        var customer = Customer.Create("Ali", "789");
        var priorReturn = Return.Create(invoice.Id, 2, 1, 180m);
        var returnItems = new[] { new ReturnItemEntry(1, 1, 200m) };

        A.CallTo(() => _loyaltyService.CalculateDiscountAsync(customer, A<decimal>._, A<CancellationToken>._))
            .Returns(0m);

        // Act
        var result = await _sut.CalculateRefundAsync(invoice, [priorReturn], returnItems, customer);

        // Assert
        Assert.Equal(160m, result.TotalRefund);
        Assert.True(result.ItemRefunds.ContainsKey(1));
    }
}
