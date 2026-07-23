using PharmaSaaS.Application.Features.Categories.Add;

namespace PharmaSaaS.Application.Test.Categories;

public class AddCategoryCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<Category> _categoryRepo = A.Fake<IGenericRepository<Category>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly AddCategoryCommandHandler _handler;

    public AddCategoryCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Categories).Returns(_categoryRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new AddCategoryCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenNameDuplicated_ReturnsDuplicatedNameError()
    {
        A.CallTo(() => _categoryRepo.AnyAsync(A<Expression<Func<Category, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new AddCategoryCommand(new CategoryRequest("Antibiotics"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CategoryErrors.DuplicatedName, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        A.CallTo(() => _categoryRepo.AnyAsync(A<Expression<Func<Category, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new AddCategoryCommand(new CategoryRequest("Antibiotics"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Antibiotics", result.Value.Name);

        A.CallTo(() => _categoryRepo.AddAsync(A<Category>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
