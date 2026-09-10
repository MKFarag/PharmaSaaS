using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Features.PurchaseOrders.Return;

namespace PharmaSaaS.Application.Test.PurchaseOrders;

public class ReturnPurchaseOrderItemsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IPurchaseOrderService _purchaseOrderService = A.Fake<IPurchaseOrderService>();

    private static ReturnPurchaseOrderItemsCommand BuildCommand(int orderId = 1, int productId = 1)
        => new(orderId, new ReturnPurchaseOrderItemsRequest([new ReturnPurchaseOrderItemRequest(productId, 2, "Damaged")]));

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

        var handler = new ReturnPurchaseOrderItemsCommandHandler(_unitOfWork, _purchaseOrderService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseOrderErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderStatusIsNotReceived_ReturnsFailure()
    {
        // Arrange
        var command = BuildCommand();
        var po = PurchaseOrder.Create(1, 1000, 0); // Status = Pending by default

        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        var handler = new ReturnPurchaseOrderItemsCommandHandler(_unitOfWork, _purchaseOrderService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseOrderErrors.InvalidStatus, result.Error);
    }

    [Fact]
    public async Task Handle_WhenProductNotBelongToOrder_ReturnsFailure()
    {
        // Arrange
        var command = BuildCommand(productId: 99); // productId 99 not in the order
        var po = PurchaseOrder.Create(1, 1000, 0);
        po.SetItems([PurchaseOrderItem.Create(1, 10, 50)]); // only productId = 1
        po.Receive();

        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        var handler = new ReturnPurchaseOrderItemsCommandHandler(_unitOfWork, _purchaseOrderService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseOrderErrors.ItemNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenServiceReturnsFailure_ReturnsFailureWithoutPersisting()
    {
        // Arrange
        var command = BuildCommand(productId: 1);
        var po = PurchaseOrder.Create(1, 1000, 0);
        po.SetItems([PurchaseOrderItem.Create(1, 10, 50)]);
        po.Receive();

        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        A.CallTo(() => _purchaseOrderService.ReturnItemsAsync(
                A<PurchaseOrder>._,
                A<List<ReturnPurchaseOrderItemRequest>>._,
                A<IUnitOfWork>._,
                A<CancellationToken>._))
            .Returns(Result.Failure(PurchaseOrderErrors.InvalidReturnQuantity));

        var handler = new ReturnPurchaseOrderItemsCommandHandler(_unitOfWork, _purchaseOrderService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseOrderErrors.InvalidReturnQuantity, result.Error);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenValidReturn_PersistsAndReturnsSuccess()
    {
        // Arrange
        var command = BuildCommand(productId: 1);
        var po = PurchaseOrder.Create(1, 1000, 0);
        po.SetItems([PurchaseOrderItem.Create(1, 10, 50)]);
        po.Receive();

        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        A.CallTo(() => _purchaseOrderService.ReturnItemsAsync(
                A<PurchaseOrder>._,
                A<List<ReturnPurchaseOrderItemRequest>>._,
                A<IUnitOfWork>._,
                A<CancellationToken>._))
            .Returns(Result.Success());

        var handler = new ReturnPurchaseOrderItemsCommandHandler(_unitOfWork, _purchaseOrderService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.True(result.IsSuccess);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
