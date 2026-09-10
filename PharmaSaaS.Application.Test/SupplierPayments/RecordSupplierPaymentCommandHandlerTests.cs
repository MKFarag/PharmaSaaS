using PharmaSaaS.Application.Contracts.SupplierPayments;
using PharmaSaaS.Application.Features.SupplierPayments.Record;

namespace PharmaSaaS.Application.Test.SupplierPayments;

public class RecordSupplierPaymentCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly ISupplierPaymentService _supplierPaymentService = A.Fake<ISupplierPaymentService>();

    private static RecordSupplierPaymentCommand BuildCommand(int supplierId = 1, List<int>? orderIds = null)
        => new(new SupplierPaymentRequest(supplierId, 500m, orderIds ?? [1, 2], "Test payment"));

    [Fact]
    public async Task Handle_WhenSupplierNotFound_ReturnsFailure()
    {
        // Arrange
        var command = BuildCommand();
        A.CallTo(() => _unitOfWork.Suppliers.GetAsync(A<object[]>._, A<CancellationToken>._))
            .Returns((Supplier?)null);

        var handler = new RecordSupplierPaymentCommandHandler(_unitOfWork, _supplierPaymentService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(SupplierErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenOrderCountMismatch_ReturnsFailure()
    {
        // Arrange
        var command = BuildCommand(supplierId: 1, orderIds: [1, 2]); // requests 2 orders
        var supplier = Supplier.Create("Test Supplier", "123");

        A.CallTo(() => _unitOfWork.Suppliers.GetAsync(A<object[]>._, A<CancellationToken>._))
            .Returns(supplier);

        // Only 1 order found instead of 2
        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAllAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<CancellationToken>._))
            .Returns([PurchaseOrder.Create(1, 500, 0)]);

        var handler = new RecordSupplierPaymentCommandHandler(_unitOfWork, _supplierPaymentService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(SupplierPaymentErrors.OrderNotBelongToSupplier, result.Error);
    }

    [Fact]
    public async Task Handle_WhenOrderBelongsToDifferentSupplier_ReturnsFailure()
    {
        // Arrange
        var command = BuildCommand(supplierId: 1, orderIds: [1]);
        var supplier = Supplier.Create("Test Supplier", "123");

        A.CallTo(() => _unitOfWork.Suppliers.GetAsync(A<object[]>._, A<CancellationToken>._))
            .Returns(supplier);

        var orderFromDifferentSupplier = PurchaseOrder.Create(supplierId: 2, 500, 0); // different supplierId
        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAllAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<CancellationToken>._))
            .Returns([orderFromDifferentSupplier]);

        var handler = new RecordSupplierPaymentCommandHandler(_unitOfWork, _supplierPaymentService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(SupplierPaymentErrors.OrderNotBelongToSupplier, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_RecordsPaymentAndReturnsSuccess()
    {
        // Arrange
        var command = BuildCommand(supplierId: 1, orderIds: [1]);
        var supplier = Supplier.Create("Test Supplier", "123");
        var order = PurchaseOrder.Create(supplierId: 1, 500, 0);

        A.CallTo(() => _unitOfWork.Suppliers.GetAsync(A<object[]>._, A<CancellationToken>._))
            .Returns(supplier);

        A.CallTo(() => _unitOfWork.PurchaseOrders.TrackedFindAllAsync(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<CancellationToken>._))
            .Returns([order]);

        var handler = new RecordSupplierPaymentCommandHandler(_unitOfWork, _supplierPaymentService);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.True(result.IsSuccess);
        A.CallTo(() => _unitOfWork.SupplierPayments.AddAsync(A<SupplierPayment>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _supplierPaymentService.DistributePaymentAmongOrders(command.Request.Amount, A<List<PurchaseOrder>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
