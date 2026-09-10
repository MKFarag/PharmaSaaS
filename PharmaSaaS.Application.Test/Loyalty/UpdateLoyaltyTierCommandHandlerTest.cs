using PharmaSaaS.Application.Contracts.Loyalty;
using PharmaSaaS.Application.Features.Loyalty.UpdateTier;

namespace PharmaSaaS.Application.Test.Loyalty;

public class UpdateLoyaltyTierCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<LoyaltyTier> _tierRepo = A.Fake<IGenericRepository<LoyaltyTier>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly UpdateLoyaltyTierCommandHandler _handler;

    public UpdateLoyaltyTierCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.LoyaltyTiers).Returns(_tierRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new UpdateLoyaltyTierCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsError()
    {
        A.CallTo(() => _tierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns((LoyaltyTier?)null);
        var result = await _handler.Handle(new UpdateLoyaltyTierCommand(1, new LoyaltyTierRequest("Gold", 1000, null)), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal(LoyaltyErrors.TierNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var tier = LoyaltyTier.Create("Silver", 500);
        A.CallTo(() => _tierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns(tier);
        A.CallTo(() => _tierRepo.AnyAsync(A<Expression<Func<LoyaltyTier, bool>>>.Ignored, A<CancellationToken>.Ignored)).Returns(false);

        var result = await _handler.Handle(new UpdateLoyaltyTierCommand(1, new LoyaltyTierRequest("Gold", 1000, null)), CancellationToken.None);
        Assert.True(result.IsSuccess);
        A.CallTo(() => _tierRepo.Update(A<LoyaltyTier>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
    }
}
