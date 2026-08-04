using FakeItEasy;
using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Features.PurchaseOrders.Create;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Entities;
using System.Linq.Expressions;
using Xunit;

namespace PharmaSaaS.Application.Test.Features.PurchaseOrders;

public class CreatePurchaseOrderCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    [Fact]
    public async Task Handle_WhenSupplierNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePurchaseOrderCommand(new PurchaseOrderRequest(1, [new PurchaseOrderItemRequest(1, 10, 150)]));
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
        var command = new CreatePurchaseOrderCommand(new PurchaseOrderRequest(1, [new PurchaseOrderItemRequest(1, 10, 150)]));
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
