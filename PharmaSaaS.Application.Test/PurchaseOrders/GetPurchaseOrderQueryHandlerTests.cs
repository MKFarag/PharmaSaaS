using PharmaSaaS.Application.Features.PurchaseOrders.Get;

namespace PharmaSaaS.Application.Test.PurchaseOrders;

public class GetPurchaseOrderQueryHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    [Fact]
    public async Task Handle_WhenPurchaseOrderNotFound_ReturnsFailure()
    {
        // Arrange
        var query = new GetPurchaseOrderQuery(1);
        A.CallTo(() => _unitOfWork.PurchaseOrders.FindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns((PurchaseOrder?)null);

        var handler = new GetPurchaseOrderQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseOrderErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderFound_ReturnsSuccess()
    {
        // Arrange
        var query = new GetPurchaseOrderQuery(1);
        var po = PurchaseOrder.Create(1, 1000, 0);
        po.SetSupplier(Supplier.Create("Test Supplier", "555"));
        po.SetItems([PurchaseOrderItem.Create(1, 5, 50)]);

        A.CallTo(() => _unitOfWork.PurchaseOrders.FindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        var handler = new GetPurchaseOrderQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.True(result.IsSuccess);
        A.CallTo(() => _unitOfWork.PurchaseOrders.FindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
