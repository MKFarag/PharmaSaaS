using PharmaSaaS.Application.Features.Categories.Get;

namespace PharmaSaaS.Application.Test.Categories;

public class GetCategoryQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<Category> _categoryRepo = A.Fake<IGenericRepository<Category>>();
    private readonly GetCategoryQueryHandler _handler;

    public GetCategoryQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Categories).Returns(_categoryRepo);
        _handler = new GetCategoryQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenCategoryFound_ReturnsSuccess()
    {
        var category = Category.Create("Antibiotics");
        category.Id = 1;

        A.CallTo(() => _categoryRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(category);

        var result = await _handler.Handle(new GetCategoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Antibiotics", result.Value.Name);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _categoryRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Category?)null);

        var result = await _handler.Handle(new GetCategoryQuery(1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CategoryErrors.NotFound, result.Error);
    }
}
