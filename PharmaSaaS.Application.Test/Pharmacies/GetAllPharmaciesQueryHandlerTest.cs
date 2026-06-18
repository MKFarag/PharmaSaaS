using PharmaSaaS.Application.Contracts.Pharmacies;
using PharmaSaaS.Application.Features.Pharmacies.GetAll;

namespace PharmaSaaS.Application.Test.Pharmacies;

public class GetAllPharmaciesQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IPharmacyRepository _pharmacyRepo = A.Fake<IPharmacyRepository>();
    private readonly GetAllPharmaciesQueryHandler _handler;

    public GetAllPharmaciesQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Pharmacies).Returns(_pharmacyRepo);
        _handler = new GetAllPharmaciesQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_ReturnsAllPharmacies()
    {
        var pharmacies = new List<PharmacyResponse>
        {
            new(1, "tenant-1", "Pharmacy A", null, null, true),
            new(2, "tenant-1", "Pharmacy B", null, null, true),
        };

        A.CallTo(() => _pharmacyRepo.GetAllAsync<PharmacyResponse>(A<CancellationToken>.Ignored))
            .Returns(pharmacies);

        var result = await _handler.Handle(new GetAllPharmaciesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, p => p.Name == "Pharmacy A");
        Assert.Contains(result, p => p.Name == "Pharmacy B");

        A.CallTo(() => _pharmacyRepo.GetAllAsync<PharmacyResponse>(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
