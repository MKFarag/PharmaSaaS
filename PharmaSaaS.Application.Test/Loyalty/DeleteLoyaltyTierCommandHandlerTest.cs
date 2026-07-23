using PharmaSaaS.Application.Features.Loyalty.DeleteTier;

namespace PharmaSaaS.Application.Test.Loyalty;

public class DeleteLoyaltyTierCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<LoyaltyTier> _tierRepo = A.Fake<IGenericRepository<LoyaltyTier>>();
    private readonly IGenericRepositoryWithPagination<Customer> _customerRepo = A.Fake<IGenericRepositoryWithPagination<Customer>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly DeleteLoyaltyTierCommandHandler _handler;

    public DeleteLoyaltyTierCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.LoyaltyTiers).Returns(_tierRepo);
        A.CallTo(() => _unitOfWork.Customers).Returns(_customerRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new DeleteLoyaltyTierCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsError()
    {
        A.CallTo(() => _tierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns((LoyaltyTier?)null);
        var result = await _handler.Handle(new DeleteLoyaltyTierCommand(1), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal(LoyaltyErrors.TierNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenInUse_ReturnsError()
    {
        var tier = LoyaltyTier.Create("Gold", 1000);
        A.CallTo(() => _tierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns(tier);
        A.CallTo(() => _customerRepo.AnyAsync(A<Expression<Func<Customer, bool>>>.Ignored, A<CancellationToken>.Ignored)).Returns(true);

        var result = await _handler.Handle(new DeleteLoyaltyTierCommand(1), CancellationToken.None);
        Assert.True(result.IsFailure);
        Assert.Equal(LoyaltyErrors.TierInUse, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var tier = LoyaltyTier.Create("Gold", 1000);
        A.CallTo(() => _tierRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored)).Returns(tier);
        A.CallTo(() => _customerRepo.AnyAsync(A<Expression<Func<Customer, bool>>>.Ignored, A<CancellationToken>.Ignored)).Returns(false);

        var result = await _handler.Handle(new DeleteLoyaltyTierCommand(1), CancellationToken.None);
        Assert.True(result.IsSuccess);
        A.CallTo(() => _tierRepo.Delete(A<LoyaltyTier>.Ignored)).MustHaveHappenedOnceExactly();
    }
}
