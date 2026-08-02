using PharmaSaaS.Application.Contracts.Suppliers;
using PharmaSaaS.Application.Features.Suppliers.Add;
using Xunit;

namespace PharmaSaaS.Application.Test.Suppliers;

public class AddSupplierCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Supplier> _supplierRepo = A.Fake<IGenericRepositoryWithPagination<Supplier>>();
    private readonly IGenericRepository<PurchaseOrder> _purchaseOrderRepo = A.Fake<IGenericRepository<PurchaseOrder>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _tenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly AddSupplierCommandHandler _handler;

    public AddSupplierCommandHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Suppliers).Returns(_supplierRepo);
        A.CallTo(() => _unitOfWork.PurchaseOrders).Returns(_purchaseOrderRepo);
        A.CallTo(() => _tenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new AddSupplierCommandHandler(_unitOfWork, _cacheService, _tenantGetter);
    }

    [Fact]
    public async Task Handle_WhenNameDuplicated_ReturnsConflictError()
    {
        // Arrange
        A.CallTo(() => _supplierRepo.AnyAsync(A<Expression<Func<Supplier, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new AddSupplierCommand(new SupplierRequest("MedPharm", null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SupplierErrors.DuplicateName, result.Error);
        A.CallTo(() => _supplierRepo.AddAsync(A<Supplier>.Ignored, A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenValid_CreatesSupplierAndInvalidatesCache()
    {
        // Arrange
        A.CallTo(() => _supplierRepo.AnyAsync(A<Expression<Func<Supplier, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        var command = new AddSupplierCommand(new SupplierRequest("MedPharm", "+20 100 000 0000"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("MedPharm", result.Value.Name);
        A.CallTo(() => _supplierRepo.AddAsync(A<Supplier>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
