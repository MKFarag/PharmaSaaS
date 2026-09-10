namespace PharmaSaaS.Application.Test.Services;

public class SupplierPaymentServiceTests
{
    private readonly SupplierPaymentService _service;

    public SupplierPaymentServiceTests()
    {
        _service = new SupplierPaymentService();
    }

    [Fact]
    public void DistributePaymentAmongOrders_ShouldDistributePaymentCorrectly_WhenAmountIsLessThanTotalRemaining()
    {
        // Arrange
        var po1 = PurchaseOrder.Create(1, 100, 0); // remaining = 100
        SetOrderDate(po1, DateTime.UtcNow.AddDays(-2));

        var po2 = PurchaseOrder.Create(1, 150, 0); // remaining = 150
        SetOrderDate(po2, DateTime.UtcNow.AddDays(-1));

        var orderList = new List<PurchaseOrder> { po1, po2 };

        // Act
        _service.DistributePaymentAmongOrders(150, orderList);

        // Assert
        Assert.Equal(100m, po1.PaidAmount);
        Assert.Equal(0m, po1.RemainingAmount);

        Assert.Equal(50m, po2.PaidAmount);
        Assert.Equal(100m, po2.RemainingAmount);
    }

    [Fact]
    public void DistributePaymentAmongOrders_ShouldStopDistributing_WhenPaymentIsExhausted()
    {
        // Arrange
        var po1 = PurchaseOrder.Create(1, 100, 0); // remaining = 100
        SetOrderDate(po1, DateTime.UtcNow.AddDays(-2));

        var po2 = PurchaseOrder.Create(1, 150, 0); // remaining = 150
        SetOrderDate(po2, DateTime.UtcNow.AddDays(-1));

        var orderList = new List<PurchaseOrder> { po1, po2 };

        // Act
        _service.DistributePaymentAmongOrders(50, orderList);

        // Assert
        Assert.Equal(50m, po1.PaidAmount);
        Assert.Equal(50m, po1.RemainingAmount);

        Assert.Equal(0m, po2.PaidAmount);
        Assert.Equal(150m, po2.RemainingAmount);
    }

    private static void SetOrderDate(PurchaseOrder order, DateTime date)
    {
        typeof(PurchaseOrder).GetProperty("OrderDate")?.SetValue(order, DateOnly.FromDateTime(date));
    }
}
