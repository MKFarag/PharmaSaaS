using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Features.PurchaseOrders.Update;

namespace PharmaSaaS.Application.Test.PurchaseOrders;

public class UpdatePurchaseOrderCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IPurchaseOrderService _purchaseOrderService = A.Fake<IPurchaseOrderService>();

    private static UpdatePurchaseOrderCommand BuildCommand(decimal? newAmount = null, List<PurchaseOrderItemRequest>? addedItems = null)
        => new(1, new UpdatePurchaseOrderRequest(newAmount, addedItems));

    [Fact]
    public async Task Handle_WhenPurchaseOrderNotFound_ReturnsFailure()
    {
        // Arrange
        var command = BuildCommand();
        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns((PurchaseOrder?)null);

        var handler = new UpdatePurchaseOrderCommandHandler(_unitOfWork, _purchaseOrderService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseOrderErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenStatusIsNotPending_ReturnsFailure()
    {
        // Arrange
        var command = BuildCommand();
        var po = PurchaseOrder.Create(1, 1000, 0);
        po.Receive(); // Status becomes Received, not Pending

        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        var handler = new UpdatePurchaseOrderCommandHandler(_unitOfWork, _purchaseOrderService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseOrderErrors.InvalidStatus, result.Error);
    }

    [Fact]
    public async Task Handle_WhenAmountProvided_AdjustsAmountAndReturnsSuccess()
    {
        // Arrange
        var command = BuildCommand(newAmount: 2000m);
        var po = PurchaseOrder.Create(1, 1000, 0);
        po.SetSupplier(Supplier.Create("Supplier", "111"));

        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        var handler = new UpdatePurchaseOrderCommandHandler(_unitOfWork, _purchaseOrderService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2000m, po.TotalAmount);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenAddedItemsProvided_DelegatesAddItemsToService()
    {
        // Arrange
        var addedItems = new List<PurchaseOrderItemRequest> { new(1, 5) };
        var command = BuildCommand(addedItems: addedItems);
        var po = PurchaseOrder.Create(1, 1000, 0);
        po.SetSupplier(Supplier.Create("Supplier", "111"));

        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        var handler = new UpdatePurchaseOrderCommandHandler(_unitOfWork, _purchaseOrderService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.True(result.IsSuccess);
        A.CallTo(() => _purchaseOrderService.AddItems(po, addedItems)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
