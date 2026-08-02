using PharmaSaaS.Application.Features.Suppliers.Delete;
using Xunit;

namespace PharmaSaaS.Application.Test.Suppliers;

public class DeleteSupplierCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Supplier> _supplierRepo = A.Fake<IGenericRepositoryWithPagination<Supplier>>();
    private readonly IGenericRepository<PurchaseOrder> _purchaseOrderRepo = A.Fake<IGenericRepository<PurchaseOrder>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _tenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly DeleteSupplierCommandHandler _handler;

    public DeleteSupplierCommandHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Suppliers).Returns(_supplierRepo);
        A.CallTo(() => _unitOfWork.PurchaseOrders).Returns(_purchaseOrderRepo);
        A.CallTo(() => _tenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new DeleteSupplierCommandHandler(_unitOfWork, _cacheService, _tenantGetter);
    }

    [Fact]
    public async Task Handle_WhenSupplierNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _supplierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Supplier?)null);

        var command = new DeleteSupplierCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SupplierErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenSupplierHasLinkedOrders_ReturnsHasOrdersError()
    {
        // Arrange
        var supplier = Supplier.Create("MedPharm", null);
        A.CallTo(() => _supplierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(supplier);
        A.CallTo(() => _purchaseOrderRepo.AnyAsync(A<Expression<Func<PurchaseOrder, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new DeleteSupplierCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SupplierErrors.HasOrders, result.Error);
        A.CallTo(() => _supplierRepo.Delete(A<Supplier>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenValid_DeletesSupplierAndInvalidatesCache()
    {
        // Arrange
        var supplier = Supplier.Create("MedPharm", null);
        A.CallTo(() => _supplierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(supplier);
        A.CallTo(() => _purchaseOrderRepo.AnyAsync(A<Expression<Func<PurchaseOrder, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        var command = new DeleteSupplierCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        A.CallTo(() => _supplierRepo.Delete(supplier))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
