using FakeItEasy;
using PharmaSaaS.Application.Features.PurchaseOrders.Receive;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Constants;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;
using System.Linq.Expressions;
using Xunit;

namespace PharmaSaaS.Application.Test.Features.PurchaseOrders;

public class ReceivePurchaseOrderCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    [Fact]
    public async Task Handle_WhenPurchaseOrderNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new ReceivePurchaseOrderCommand(1);
        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(A<Expression<Func<PurchaseOrder, bool>>>._, A<string[]>._, A<CancellationToken>._)).Returns((PurchaseOrder?)null);

        var handler = new ReceivePurchaseOrderCommandHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseOrderErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderAlreadyReceived_ReturnsFailure()
    {
        // Arrange
        var command = new ReceivePurchaseOrderCommand(1);
        var po = PurchaseOrder.Create(1, 100);
        po.Receive(); // Sets status to Received
        
        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(A<Expression<Func<PurchaseOrder, bool>>>._, A<string[]>._, A<CancellationToken>._)).Returns(po);

        var handler = new ReceivePurchaseOrderCommandHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseOrderErrors.AlreadyReceived, result.Error);
    }
}
