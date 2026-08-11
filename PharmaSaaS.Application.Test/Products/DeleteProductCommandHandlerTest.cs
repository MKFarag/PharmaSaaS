using PharmaSaaS.Application.Features.Products.Delete;

namespace PharmaSaaS.Application.Test.Products;

public class DeleteProductCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IProductRepository _productRepo = A.Fake<IProductRepository>();
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Products).Returns(_productRepo);
        _handler = new DeleteProductCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _productRepo.GetAsync(
                A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Product?)null);

        var result = await _handler.Handle(new DeleteProductCommand(99), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var product = Product.Create("Amoxicillin", 1, 1);
        product.Id = 1;

        A.CallTo(() => _productRepo.GetAsync(
                A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(product);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored)).Returns(1);

        var result = await _handler.Handle(new DeleteProductCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        A.CallTo(() => _productRepo.Delete(product))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
