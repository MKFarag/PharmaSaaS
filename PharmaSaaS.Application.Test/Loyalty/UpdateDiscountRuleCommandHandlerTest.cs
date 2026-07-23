using PharmaSaaS.Application.Contracts.Loyalty;
using PharmaSaaS.Application.Features.Loyalty.UpdateRule;

namespace PharmaSaaS.Application.Test.Loyalty;

public class UpdateDiscountRuleCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<DiscountRule> _ruleRepo = A.Fake<IGenericRepository<DiscountRule>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly UpdateDiscountRuleCommandHandler _handler;

    public UpdateDiscountRuleCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.DiscountRules).Returns(_ruleRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new UpdateDiscountRuleCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsError()
    {
        A.CallTo(() => _ruleRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns((DiscountRule?)null);
        var result = await _handler.Handle(new UpdateDiscountRuleCommand(1, new DiscountRuleRequest(100, 10)), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal(LoyaltyErrors.RuleNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenAmountDuplicated_ReturnsError()
    {
        var rule = DiscountRule.Create(1, 50, 5);
        A.CallTo(() => _ruleRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns(rule);
        A.CallTo(() => _ruleRepo.AnyAsync(A<Expression<Func<DiscountRule, bool>>>.Ignored, A<CancellationToken>.Ignored)).Returns(true);

        var result = await _handler.Handle(new UpdateDiscountRuleCommand(1, new DiscountRuleRequest(100, 10)), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal(LoyaltyErrors.DuplicatedRuleAmount, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var rule = DiscountRule.Create(1, 50, 5);
        A.CallTo(() => _ruleRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns(rule);
        A.CallTo(() => _ruleRepo.AnyAsync(A<Expression<Func<DiscountRule, bool>>>.Ignored, A<CancellationToken>.Ignored)).Returns(false);

        var result = await _handler.Handle(new UpdateDiscountRuleCommand(1, new DiscountRuleRequest(100, 10)), CancellationToken.None);
        Assert.True(result.IsSuccess);
        A.CallTo(() => _ruleRepo.Update(A<DiscountRule>.Ignored)).MustHaveHappenedOnceExactly();
    }
}
