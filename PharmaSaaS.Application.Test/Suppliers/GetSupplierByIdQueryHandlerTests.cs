using PharmaSaaS.Application.Features.Suppliers.Get;

namespace PharmaSaaS.Application.Test.Suppliers;

public class GetSupplierByIdQueryHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Supplier> _supplierRepo = A.Fake<IGenericRepositoryWithPagination<Supplier>>();
    private readonly GetSupplierByIdQueryHandler _handler;

    public GetSupplierByIdQueryHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Suppliers).Returns(_supplierRepo);
        _handler = new GetSupplierByIdQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenSupplierNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _supplierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Supplier?)null);

        var query = new GetSupplierByIdQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(SupplierErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenFound_ReturnsMappedResponse()
    {
        // Arrange
        var supplier = Supplier.Create("MedPharm", "+20 100 000 0000");

        A.CallTo(() => _supplierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(supplier);

        var query = new GetSupplierByIdQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("MedPharm", result.Value.Name);
        Assert.Equal("+20 100 000 0000", result.Value.ContactInfo);
    }
}
