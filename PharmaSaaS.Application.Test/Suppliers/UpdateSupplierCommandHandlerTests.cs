using PharmaSaaS.Application.Features.Suppliers.Update;

namespace PharmaSaaS.Application.Test.Suppliers;

public class UpdateSupplierCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Supplier> _supplierRepo = A.Fake<IGenericRepositoryWithPagination<Supplier>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _tenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly UpdateSupplierCommandHandler _handler;

    public UpdateSupplierCommandHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Suppliers).Returns(_supplierRepo);
        A.CallTo(() => _tenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new UpdateSupplierCommandHandler(_unitOfWork, _cacheService, _tenantGetter);
    }

    [Fact]
    public async Task Handle_WhenSupplierNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _supplierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Supplier?)null);

        var command = new UpdateSupplierCommand(1, new SupplierRequest("NewName", null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SupplierErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenNameTakenByAnotherSupplier_ReturnsConflictError()
    {
        // Arrange
        var supplier = Supplier.Create("OldName", null);
        A.CallTo(() => _supplierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(supplier);
        A.CallTo(() => _supplierRepo.AnyAsync(A<Expression<Func<Supplier, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new UpdateSupplierCommand(1, new SupplierRequest("TakenName", null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SupplierErrors.DuplicateName, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesSupplierAndInvalidatesCache()
    {
        // Arrange
        var supplier = Supplier.Create("OldName", null);
        A.CallTo(() => _supplierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(supplier);
        A.CallTo(() => _supplierRepo.AnyAsync(A<Expression<Func<Supplier, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        var command = new UpdateSupplierCommand(1, new SupplierRequest("NewName", "+20 100 000 0000"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("NewName", supplier.Name);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
