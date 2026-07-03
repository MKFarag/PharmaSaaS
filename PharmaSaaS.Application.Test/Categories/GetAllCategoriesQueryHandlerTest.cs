using PharmaSaaS.Application.Features.Categories.GetAll;

namespace PharmaSaaS.Application.Test.Categories;

public class GetAllCategoriesQueryHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<Category> _categoryRepo = A.Fake<IGenericRepository<Category>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly GetAllCategoriesQueryHandler _handler;

    public GetAllCategoriesQueryHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Categories).Returns(_categoryRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new GetAllCategoriesQueryHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_ReturnsAllCategories()
    {
        var categories = new List<CategoryResponse>
        {
            new(1, "Antibiotics"),
            new(2, "Analgesics")
        };

        A.CallTo(() => _cacheService.GetOrCreateAsync(
                A<string>.Ignored,
                A<Func<CancellationToken, Task<IEnumerable<CategoryResponse>>>>.Ignored,
                A<TimeSpan?>.Ignored,
                A<IEnumerable<string>?>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(categories);

        var result = await _handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.Name == "Antibiotics");
    }
}
