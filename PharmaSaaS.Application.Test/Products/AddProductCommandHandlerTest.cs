using PharmaSaaS.Application.Features.Products.Add;

namespace PharmaSaaS.Application.Test.Products;

public class AddProductCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IProductRepository _productRepo = A.Fake<IProductRepository>();
    private readonly IGenericRepository<Category> _categoryRepo = A.Fake<IGenericRepository<Category>>();
    private readonly IGenericRepository<DosageForm> _dosageFormRepo = A.Fake<IGenericRepository<DosageForm>>();
    private readonly AddProductCommandHandler _handler;

    public AddProductCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Products).Returns(_productRepo);
        A.CallTo(() => _unitOfWork.Categories).Returns(_categoryRepo);
        A.CallTo(() => _unitOfWork.DosageForms).Returns(_dosageFormRepo);
        _handler = new AddProductCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenBarcodeDuplicated_ReturnsDuplicatedBarcodeError()
    {
        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var result = await _handler.Handle(
            new AddProductCommand(new ProductRequest("Amoxicillin", "6291040100216", null, null, 1, 1)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.DuplicatedBarcode, result.Error);
    }

    [Fact]
    public async Task Handle_WhenCompositeDuplicate_ReturnsCompositeDuplicateError()
    {
        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var result = await _handler.Handle(
            new AddProductCommand(new ProductRequest("Amoxicillin", null, "500 mg", null, 1, 1)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.CompositeDuplicate, result.Error);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ReturnsCategoryNotFoundError()
    {
        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);
        A.CallTo(() => _categoryRepo.AnyAsync(A<Expression<Func<Category, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        var result = await _handler.Handle(
            new AddProductCommand(new ProductRequest("Amoxicillin", null, null, null, 99, 1)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.CategoryNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenDosageFormNotFound_ReturnsDosageFormNotFoundError()
    {
        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);
        A.CallTo(() => _categoryRepo.AnyAsync(A<Expression<Func<Category, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);
        A.CallTo(() => _dosageFormRepo.AnyAsync(A<Expression<Func<DosageForm, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        var result = await _handler.Handle(
            new AddProductCommand(new ProductRequest("Amoxicillin", null, null, null, 1, 99)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.DosageFormNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);
        A.CallTo(() => _categoryRepo.AnyAsync(A<Expression<Func<Category, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);
        A.CallTo(() => _dosageFormRepo.AnyAsync(A<Expression<Func<DosageForm, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored)).Returns(1);

        var result = await _handler.Handle(
            new AddProductCommand(new ProductRequest("Amoxicillin", "6291040100216", "500 mg", 25.0m, 1, 1)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        A.CallTo(() => _productRepo.AddAsync(A<Product>.That.Matches(
            p => p.Name == "Amoxicillin" && p.Barcode == "6291040100216"), A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
