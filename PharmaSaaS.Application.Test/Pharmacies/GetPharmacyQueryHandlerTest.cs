using PharmaSaaS.Application.Features.Pharmacies.Get;

namespace PharmaSaaS.Application.Test.Pharmacies;

public class GetPharmacyQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IPharmacyRepository _pharmacyRepo = A.Fake<IPharmacyRepository>();
    private readonly GetPharmacyQueryHandler _handler;

    public GetPharmacyQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Pharmacies).Returns(_pharmacyRepo);
        _handler = new GetPharmacyQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenPharmacyFound_ReturnsSuccess()
    {
        var pharmacy = Pharmacy.Create("My Pharmacy");
        pharmacy.Id = 1;
        pharmacy.ToggleStatus();

        A.CallTo(() => _pharmacyRepo.GetAsync(A<int>.Ignored, A<CancellationToken>.Ignored))
            .Returns(pharmacy);

        var result = await _handler.Handle(new GetPharmacyQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal(pharmacy.TenantId, result.Value.TenantId);
        Assert.Equal("My Pharmacy", result.Value.Name);
        Assert.True(result.Value.IsActive);
    }

    [Fact]
    public async Task Handle_WhenPharmacyNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _pharmacyRepo.GetAsync(A<int>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Pharmacy?)null);

        var result = await _handler.Handle(new GetPharmacyQuery(1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PharmacyErrors.NotFound, result.Error);
    }
}
