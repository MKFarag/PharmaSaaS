using PharmaSaaS.Application.Features.Products.GetAll;

namespace PharmaSaaS.Application.Test.Products;

public class GetAllProductsQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Product> _productRepo = A.Fake<IGenericRepositoryWithPagination<Product>>();
    private readonly GetAllProductsQueryHandler _handler;

    public GetAllProductsQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Products).Returns(_productRepo);
        _handler = new GetAllProductsQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedList()
    {
        var items = new List<ProductResponse>
        {
            new(1, "Amoxicillin", null, null, null,
                new CategoryResponse(1, "Antibiotics"),
                new DosageFormResponse(1, "Tablet"))
        };
        var paginatedList = A.Fake<IPaginatedList<ProductResponse>>();
        A.CallTo(() => paginatedList.Items).Returns(items);
        A.CallTo(() => paginatedList.TotalPages).Returns(1);
        A.CallTo(() => paginatedList.PageNumber).Returns(1);

        A.CallTo(() => _productRepo.FindPaginatedListAsync<ProductResponse>(
                A<Expression<Func<Product, bool>>>.Ignored,
                A<int>.Ignored, A<int>.Ignored,
                A<string?>.Ignored, A<string>.Ignored,
                A<string>.Ignored!, A<string>.Ignored!,
                A<ColumnType>.Ignored,
                A<string[]>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(paginatedList);

        var result = await _handler.Handle(
            new GetAllProductsQuery(new ProductRequestFilters()), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalPages);
    }
}
