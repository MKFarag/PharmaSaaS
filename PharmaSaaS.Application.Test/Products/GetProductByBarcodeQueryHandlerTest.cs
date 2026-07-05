using PharmaSaaS.Application.Features.Products.GetByBarcode;

namespace PharmaSaaS.Application.Test.Products;

public class GetProductByBarcodeQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Product> _productRepo = A.Fake<IGenericRepositoryWithPagination<Product>>();
    private readonly GetProductByBarcodeQueryHandler _handler;

    public GetProductByBarcodeQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Products).Returns(_productRepo);
        _handler = new GetProductByBarcodeQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenProductFound_ReturnsSuccess()
    {
        var category = Category.Create("Antibiotics");
        category.Id = 1;
        var dosageForm = DosageForm.Create("Tablet");
        dosageForm.Id = 1;
        var product = Product.Create("Amoxicillin", 1, 1, "6291040100216");
        product.Id = 1;
        product.Category = category;
        product.DosageForm = dosageForm;

        A.CallTo(() => _productRepo.FindAsync(
                A<Expression<Func<Product, bool>>>.Ignored,
                A<string[]>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(product);

        var result = await _handler.Handle(new GetProductByBarcodeQuery("6291040100216"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Amoxicillin", result.Value.Name);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _productRepo.FindAsync(
                A<Expression<Func<Product, bool>>>.Ignored,
                A<string[]>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns((Product?)null);

        var result = await _handler.Handle(new GetProductByBarcodeQuery("invalid-barcode"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NotFound, result.Error);
    }
}
