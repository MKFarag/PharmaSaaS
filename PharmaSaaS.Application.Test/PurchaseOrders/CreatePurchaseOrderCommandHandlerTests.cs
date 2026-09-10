using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Features.PurchaseOrders.Create;

namespace PharmaSaaS.Application.Test.PurchaseOrders;

public class CreatePurchaseOrderCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    private static CreatePurchaseOrderCommand BuildCommand()
        => new(new PurchaseOrderRequest(1, [new PurchaseOrderItemRequest(1, 10)], 1500));

    [Fact]
    public async Task Handle_WhenSupplierNotFound_ReturnsFailure()
    {
        // Arrange
        var command = BuildCommand();
        A.CallTo(() => _unitOfWork.Suppliers.GetAsync(A<object[]>._, A<CancellationToken>._)).Returns((Supplier?)null);

        var handler = new CreatePurchaseOrderCommandHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Supplier.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenProductsNotFound_ReturnsFailure()
    {
        // Arrange
        var command = BuildCommand();
        var supplier = Supplier.Create("Test", null);
        A.CallTo(() => _unitOfWork.Suppliers.GetAsync(A<object[]>._, A<CancellationToken>._)).Returns(supplier);
        A.CallTo(() => _unitOfWork.Products.CountAsync(A<Expression<Func<Product, bool>>>._, A<CancellationToken>._)).Returns(0);

        var handler = new CreatePurchaseOrderCommandHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Product.NotFound", result.Error.Code);
    }

}
