using PharmaSaaS.Application.Features.Suppliers.ResetBalance;

namespace PharmaSaaS.Application.Test.Suppliers;

public class ResetSupplierBalanceCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Supplier> _supplierRepo = A.Fake<IGenericRepositoryWithPagination<Supplier>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _tenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly ResetSupplierBalanceCommandHandler _handler;

    public ResetSupplierBalanceCommandHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Suppliers).Returns(_supplierRepo);
        A.CallTo(() => _tenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new ResetSupplierBalanceCommandHandler(_unitOfWork, _cacheService, _tenantGetter);
    }

    [Fact]
    public async Task Handle_WhenSupplierNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _supplierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Supplier?)null);

        var command = new ResetSupplierBalanceCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SupplierErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValid_ResetsBalanceToZeroAndInvalidatesCache()
    {
        // Arrange
        var supplier = Supplier.Create("MedPharm", null);
        supplier.AddOrder(3000m); // Simulate an outstanding balance
        Assert.Equal(-3000m, supplier.Balance); // Verify precondition

        A.CallTo(() => _supplierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(supplier);

        var command = new ResetSupplierBalanceCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0m, supplier.Balance);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
