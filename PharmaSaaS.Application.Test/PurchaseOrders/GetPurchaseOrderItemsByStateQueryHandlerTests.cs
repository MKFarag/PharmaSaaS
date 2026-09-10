using PharmaSaaS.Application.Features.PurchaseOrders.GetItems;

namespace PharmaSaaS.Application.Test.PurchaseOrders;

public class GetPurchaseOrderItemsByStateQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    [Fact]
    public async Task Handle_WhenPurchaseOrderNotFound_ReturnsFailure()
    {
        // Arrange
        var query = new GetPurchaseOrderItemsByStateQuery(1, PurchaseOrderItemStatus.Pending);
        A.CallTo(() => _unitOfWork.PurchaseOrders.FindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns((PurchaseOrder?)null);

        var handler = new GetPurchaseOrderItemsByStateQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseOrderErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderFound_ReturnsOnlyItemsMatchingRequestedState()
    {
        // Arrange
        var item1 = PurchaseOrderItem.Create(1, 10, 50); // Pending by default
        var item2 = PurchaseOrderItem.Create(2, 5, 30);  // Pending by default

        var product1 = Product.Create("Product A", 1, 1);
        var product2 = Product.Create("Product B", 1, 1);
        typeof(PurchaseOrderItem).GetProperty(nameof(PurchaseOrderItem.Product))?.SetValue(item1, product1);
        typeof(PurchaseOrderItem).GetProperty(nameof(PurchaseOrderItem.Product))?.SetValue(item2, product2);

        var po = PurchaseOrder.Create(1, 1000, 0);
        po.SetItems([item1, item2]);

        var query = new GetPurchaseOrderItemsByStateQuery(1, PurchaseOrderItemStatus.Pending);

        A.CallTo(() => _unitOfWork.PurchaseOrders.FindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        var handler = new GetPurchaseOrderItemsByStateQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, item => Assert.Equal(PurchaseOrderItemStatus.Pending.ToString(), item.Status));
    }

    [Fact]
    public async Task Handle_WhenNoItemsMatchState_ReturnsEmptyList()
    {
        // Arrange
        var item1 = PurchaseOrderItem.Create(1, 10, 50); // Pending by default
        var product1 = Product.Create("Product A", 1, 1);
        typeof(PurchaseOrderItem).GetProperty(nameof(PurchaseOrderItem.Product))?.SetValue(item1, product1);

        var po = PurchaseOrder.Create(1, 1000, 0);
        po.SetItems([item1]);

        // Query for Received items, but all items are Pending
        var query = new GetPurchaseOrderItemsByStateQuery(1, PurchaseOrderItemStatus.Received);

        A.CallTo(() => _unitOfWork.PurchaseOrders.FindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        var handler = new GetPurchaseOrderItemsByStateQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
