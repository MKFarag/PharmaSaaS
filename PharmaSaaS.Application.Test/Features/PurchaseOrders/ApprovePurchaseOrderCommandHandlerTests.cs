using FakeItEasy;
using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Features.PurchaseOrders.Approve;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Constants;
using PharmaSaaS.Domain.Entities;
using Xunit;
using System.Linq.Expressions;

using PharmaSaaS.Domain.Enums;

namespace PharmaSaaS.Application.Test.Features.PurchaseOrders;

public class ApprovePurchaseOrderCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    [Fact]
    public async Task Handle_WhenValidRequest_ApprovesOrder()
    {
        // Arrange
        var request = new ApprovePurchaseOrderRequest(1500m, [
            new ApprovePurchaseOrderItemRequest(1, 10, 100)
        ]);
        var command = new ApprovePurchaseOrderCommand(1, request);

        var po = PurchaseOrder.Create(1, 1000, 0);
        po.SetSupplier(Supplier.Create("Supplier A", "123"));
        po.SetItems([PurchaseOrderItem.Create(1, 10, 0)]);

        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(po);

        var handler = new ApprovePurchaseOrderCommandHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(PurchaseOrderStatus.Received, po.Status);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}

