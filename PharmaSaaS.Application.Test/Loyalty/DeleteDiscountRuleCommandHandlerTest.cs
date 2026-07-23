using PharmaSaaS.Application.Features.Loyalty.DeleteRule;

namespace PharmaSaaS.Application.Test.Loyalty;

public class DeleteDiscountRuleCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<DiscountRule> _ruleRepo = A.Fake<IGenericRepository<DiscountRule>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly DeleteDiscountRuleCommandHandler _handler;

    public DeleteDiscountRuleCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.DiscountRules).Returns(_ruleRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new DeleteDiscountRuleCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsError()
    {
        A.CallTo(() => _ruleRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns((DiscountRule?)null);
        var result = await _handler.Handle(new DeleteDiscountRuleCommand(1), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal(LoyaltyErrors.RuleNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var rule = DiscountRule.Create(1, 50, 5);
        A.CallTo(() => _ruleRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns(rule);

        var result = await _handler.Handle(new DeleteDiscountRuleCommand(1), CancellationToken.None);
        Assert.True(result.IsSuccess);
        A.CallTo(() => _ruleRepo.Delete(A<DiscountRule>.Ignored)).MustHaveHappenedOnceExactly();
    }
}
