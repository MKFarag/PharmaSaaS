using PharmaSaaS.Application.Contracts.Suppliers;
using PharmaSaaS.Application.Features.Suppliers.GetAll;
using Xunit;

namespace PharmaSaaS.Application.Test.Suppliers;

public class GetAllSuppliersQueryHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Supplier> _supplierRepo = A.Fake<IGenericRepositoryWithPagination<Supplier>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _tenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly GetAllSuppliersQueryHandler _handler;

    public GetAllSuppliersQueryHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Suppliers).Returns(_supplierRepo);
        A.CallTo(() => _tenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new GetAllSuppliersQueryHandler(_unitOfWork, _cacheService, _tenantGetter);
    }

    [Fact]
    public async Task Handle_ReturnsCachedList()
    {
        // Arrange
        var list = new List<SupplierResponse>();

        A.CallTo(() => _cacheService.GetOrCreateAsync(
            Cache.Keys.AllSuppliers("tenant-1"),
            A<Func<CancellationToken, Task<IEnumerable<SupplierResponse>>>>.Ignored,
            Cache.Expirations.VeryLong,
            A<IEnumerable<string>>.That.IsSameSequenceAs(new[] { Cache.Tags.Suppliers("tenant-1") }),
            A<CancellationToken>.Ignored))
            .Returns(list);

        var query = new GetAllSuppliersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
