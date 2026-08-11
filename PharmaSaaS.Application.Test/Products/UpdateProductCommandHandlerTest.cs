using PharmaSaaS.Application.Features.Products.Update;

namespace PharmaSaaS.Application.Test.Products;

public class UpdateProductCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IProductRepository _productRepo = A.Fake<IProductRepository>();
    private readonly IGenericRepository<Category> _categoryRepo = A.Fake<IGenericRepository<Category>>();
    private readonly IGenericRepository<DosageForm> _dosageFormRepo = A.Fake<IGenericRepository<DosageForm>>();
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Products).Returns(_productRepo);
        A.CallTo(() => _unitOfWork.Categories).Returns(_categoryRepo);
        A.CallTo(() => _unitOfWork.DosageForms).Returns(_dosageFormRepo);
        _handler = new UpdateProductCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _productRepo.GetAsync(
                A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Product?)null);

        var result = await _handler.Handle(
            new UpdateProductCommand(99, new ProductRequest("Amoxicillin", null, null, null, 1, 1)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenBarcodeDuplicated_ReturnsDuplicatedBarcodeError()
    {
        var product = Product.Create("Amoxicillin", 1, 1);
        product.Id = 1;

        A.CallTo(() => _productRepo.GetAsync(
                A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(product);
        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var result = await _handler.Handle(
            new UpdateProductCommand(1, new ProductRequest("Amoxicillin", "6291040100216", null, null, 1, 1)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.DuplicatedBarcode, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var product = Product.Create("Amoxicillin", 1, 1);
        product.Id = 1;

        A.CallTo(() => _productRepo.GetAsync(
                A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(product);
        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);
        A.CallTo(() => _categoryRepo.AnyAsync(A<Expression<Func<Category, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);
        A.CallTo(() => _dosageFormRepo.AnyAsync(A<Expression<Func<DosageForm, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored)).Returns(1);

        var result = await _handler.Handle(
            new UpdateProductCommand(1, new ProductRequest("Amoxicillin Updated", null, "500 mg", 25.0m, 2, 2)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
