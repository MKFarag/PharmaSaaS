using PharmaSaaS.Application.Contracts.Loyalty;
using PharmaSaaS.Application.Features.Loyalty.AddRule;

namespace PharmaSaaS.Application.Test.Loyalty;

public class AddDiscountRuleCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<DiscountRule> _ruleRepo = A.Fake<IGenericRepository<DiscountRule>>();
    private readonly IGenericRepository<LoyaltyTier> _tierRepo = A.Fake<IGenericRepository<LoyaltyTier>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly AddDiscountRuleCommandHandler _handler;

    public AddDiscountRuleCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.DiscountRules).Returns(_ruleRepo);
        A.CallTo(() => _unitOfWork.LoyaltyTiers).Returns(_tierRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new AddDiscountRuleCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenTierNotFound_ReturnsError()
    {
        A.CallTo(() => _tierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns((LoyaltyTier?)null);
        var result = await _handler.Handle(new AddDiscountRuleCommand(1, new DiscountRuleRequest(100, 10)), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal(LoyaltyErrors.TierNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenAmountDuplicated_ReturnsError()
    {
        A.CallTo(() => _tierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns(LoyaltyTier.Create("T", 10));
        A.CallTo(() => _ruleRepo.AnyAsync(A<Expression<Func<DiscountRule, bool>>>.Ignored, A<CancellationToken>.Ignored)).Returns(true);
        var result = await _handler.Handle(new AddDiscountRuleCommand(1, new DiscountRuleRequest(100, 10)), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal(LoyaltyErrors.DuplicatedRuleAmount, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        A.CallTo(() => _tierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns(LoyaltyTier.Create("T", 10));
        A.CallTo(() => _ruleRepo.AnyAsync(A<Expression<Func<DiscountRule, bool>>>.Ignored, A<CancellationToken>.Ignored)).Returns(false);
        var result = await _handler.Handle(new AddDiscountRuleCommand(1, new DiscountRuleRequest(100, 10)), CancellationToken.None);
        Assert.True(result.IsSuccess);
        A.CallTo(() => _ruleRepo.AddAsync(A<DiscountRule>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
    }
}
