using PharmaSaaS.Application.Features.Categories.Delete;

namespace PharmaSaaS.Application.Test.Categories;

public class DeleteCategoryCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<Category> _categoryRepo = A.Fake<IGenericRepository<Category>>();
    private readonly IProductRepository _productRepo = A.Fake<IProductRepository>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Categories).Returns(_categoryRepo);
        A.CallTo(() => _unitOfWork.Products).Returns(_productRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new DeleteCategoryCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _categoryRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Category?)null);

        var result = await _handler.Handle(new DeleteCategoryCommand(1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CategoryErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenCategoryIsUsed_ReturnsIsUsedError()
    {
        var category = Category.Create("Antibiotics");
        category.Id = 1;

        A.CallTo(() => _categoryRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(category);

        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var result = await _handler.Handle(new DeleteCategoryCommand(1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CategoryErrors.IsUsed, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var category = Category.Create("Antibiotics");
        category.Id = 1;

        A.CallTo(() => _categoryRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(category);

        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new DeleteCategoryCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);

        A.CallTo(() => _categoryRepo.Delete(A<Category>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
