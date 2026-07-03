using PharmaSaaS.Application.Contracts.Categories;
using PharmaSaaS.Application.Features.Categories.Update;

namespace PharmaSaaS.Application.Test.Categories;

public class UpdateCategoryCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<Category> _categoryRepo = A.Fake<IGenericRepository<Category>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Categories).Returns(_categoryRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new UpdateCategoryCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _categoryRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Category?)null);

        var command = new UpdateCategoryCommand(1, new CategoryRequest("New Name"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CategoryErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenNameDuplicated_ReturnsDuplicatedNameError()
    {
        var category = Category.Create("Old Name");
        category.Id = 1;

        A.CallTo(() => _categoryRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(category);

        A.CallTo(() => _categoryRepo.AnyAsync(A<Expression<Func<Category, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new UpdateCategoryCommand(1, new CategoryRequest("Existing Name"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CategoryErrors.DuplicatedName, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var category = Category.Create("Old Name");
        category.Id = 1;

        A.CallTo(() => _categoryRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(category);

        A.CallTo(() => _categoryRepo.AnyAsync(A<Expression<Func<Category, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new UpdateCategoryCommand(1, new CategoryRequest("New Name"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
