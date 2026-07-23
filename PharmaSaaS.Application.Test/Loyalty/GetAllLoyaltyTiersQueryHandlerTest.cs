using PharmaSaaS.Application.Contracts.Loyalty;
using PharmaSaaS.Application.Features.Loyalty.GetAll;

namespace PharmaSaaS.Application.Test.Loyalty;

public class GetAllLoyaltyTiersQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<LoyaltyTier> _tierRepo = A.Fake<IGenericRepository<LoyaltyTier>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly GetAllLoyaltyTiersQueryHandler _handler;

    public GetAllLoyaltyTiersQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.LoyaltyTiers).Returns(_tierRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new GetAllLoyaltyTiersQueryHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_ReturnsCachedList()
    {
        var tiers = new List<LoyaltyTier> { LoyaltyTier.Create("Gold", 1000) };
        A.CallTo(() => _cacheService.GetOrCreateAsync(
            A<string>.Ignored,
            A<Func<CancellationToken, Task<IEnumerable<LoyaltyTierResponse>>>>.Ignored,
            A<TimeSpan?>.Ignored,
            A<string[]>.Ignored,
            A<CancellationToken>.Ignored))
            .Returns(new List<LoyaltyTierResponse>());

        var result = await _handler.Handle(new GetAllLoyaltyTiersQuery(), CancellationToken.None);

        Assert.NotNull(result);
    }
}
