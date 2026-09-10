using PharmaSaaS.Application.Contracts.PurchaseOrders;

namespace PharmaSaaS.Application.Test.Services;

public class PurchaseOrderServiceTests
{
    private readonly PurchaseOrderService _service;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseOrderServiceTests()
    {
        _service = new PurchaseOrderService();
        _unitOfWork = A.Fake<IUnitOfWork>();
    }

    [Fact]
    public async Task ApproveItemsAsync_ShouldAddStock_WhenItemsAreReceived()
    {
        // Arrange
        var po = PurchaseOrder.Create(1, 100, 0);
        po.SetItems(new List<PurchaseOrderItem>());
        po.Items.Add(PurchaseOrderItem.Create(1, 10)); // ProductId=1, Qty=10

        var requestedItems = new List<ApprovePurchaseOrderItemRequest>
        {
            new(1, 10, 10m, false) // received 10, unit cost 10
        };

        var inventory = Inventory.Create(1, 50, 20);

        A.CallTo(() => _unitOfWork.Inventories.TrackedFindAsync(
                A<Expression<Func<Inventory, bool>>>._,
                A<CancellationToken>._))
            .Returns(inventory);

        // Act
        var result = await _service.ApproveItemsAsync(po, requestedItems, _unitOfWork);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PharmaSaaS.Domain.Enums.PurchaseOrderItemStatus.Received, po.Items.First().Status);
        Assert.Equal(10, po.Items.First().ReceivedQuantity);
        Assert.Equal(60, inventory.Quantity); // 50 + 10
    }

    [Fact]
    public async Task ApproveItemsAsync_ShouldReturnFailure_WhenRequestedItemWasNotOrdered()
    {
        // Arrange
        var po = PurchaseOrder.Create(1, 100, 0);
        po.SetItems(new List<PurchaseOrderItem>());
        po.Items.Add(PurchaseOrderItem.Create(1, 10)); // Ordered ProductId=1

        // Requesting approval for ProductId=2 which is not in the original PO
        var requestedItems = new List<ApprovePurchaseOrderItemRequest>
        {
            new(2, 5, 10m, false)
        };

        // Act
        var result = await _service.ApproveItemsAsync(po, requestedItems, _unitOfWork);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("PurchaseOrder.ItemNotFound", result.Error.Code);
    }

    [Fact]
    public void AddItems_ShouldAddNewItemsAndCombineQuantitiesForExisting()
    {
        // Arrange
        var po = PurchaseOrder.Create(1, 100, 0);
        po.SetItems(new List<PurchaseOrderItem>());
        po.Items.Add(PurchaseOrderItem.Create(1, 10)); // Existing item Qty=10

        var requestedItems = new List<PurchaseOrderItemRequest>
        {
            new(1, 5), // Add 5 more to existing
            new(2, 20) // Add new item
        };

        // Act
        _service.AddItems(po, requestedItems);

        // Assert
        Assert.Equal(2, po.Items.Count);

        var item1 = po.Items.First(i => i.ProductId == 1);
        Assert.Equal(15, item1.Quantity);

        var item2 = po.Items.First(i => i.ProductId == 2);
        Assert.Equal(20, item2.Quantity);
    }
}
