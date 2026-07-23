using PharmaSaaS.Application.Contracts.Loyalty;
using PharmaSaaS.Application.Features.Loyalty.AddTier;

namespace PharmaSaaS.Application.Test.Loyalty;

public class AddLoyaltyTierCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<LoyaltyTier> _tierRepo = A.Fake<IGenericRepository<LoyaltyTier>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly AddLoyaltyTierCommandHandler _handler;

    public AddLoyaltyTierCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.LoyaltyTiers).Returns(_tierRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new AddLoyaltyTierCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenNameDuplicated_ReturnsError()
    {
        A.CallTo(() => _tierRepo.AnyAsync(A<Expression<Func<LoyaltyTier, bool>>>.Ignored, A<CancellationToken>.Ignored)).Returns(true);
        var result = await _handler.Handle(new AddLoyaltyTierCommand(new LoyaltyTierRequest("Gold", 1000)), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal(LoyaltyErrors.DuplicatedTierName, result.Error);
    }

    [Fact]
    public async Task Handle_WhenRuleAmountsDuplicated_ReturnsError()
    {
        A.CallTo(() => _tierRepo.AnyAsync(A<Expression<Func<LoyaltyTier, bool>>>.Ignored, A<CancellationToken>.Ignored)).Returns(false);
        var req = new LoyaltyTierRequest("Gold", 1000, new[] { new DiscountRuleRequest(100, 10), new DiscountRuleRequest(100, 20) });
        var result = await _handler.Handle(new AddLoyaltyTierCommand(req), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal(LoyaltyErrors.DuplicatedRuleAmount, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        A.CallTo(() => _tierRepo.AnyAsync(A<Expression<Func<LoyaltyTier, bool>>>.Ignored, A<CancellationToken>.Ignored)).Returns(false);
        var result = await _handler.Handle(new AddLoyaltyTierCommand(new LoyaltyTierRequest("Gold", 1000)), CancellationToken.None);
        Assert.True(result.IsSuccess);
        A.CallTo(() => _tierRepo.AddAsync(A<LoyaltyTier>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
    }
}
