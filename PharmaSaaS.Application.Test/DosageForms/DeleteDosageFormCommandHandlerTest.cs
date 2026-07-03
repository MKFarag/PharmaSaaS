using PharmaSaaS.Application.Features.DosageForms.Delete;

namespace PharmaSaaS.Application.Test.DosageForms;

public class DeleteDosageFormCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<DosageForm> _dosageFormRepo = A.Fake<IGenericRepository<DosageForm>>();
    private readonly IGenericRepository<Product> _productRepo = A.Fake<IGenericRepository<Product>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly DeleteDosageFormCommandHandler _handler;

    public DeleteDosageFormCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.DosageForms).Returns(_dosageFormRepo);
        A.CallTo(() => _unitOfWork.Products).Returns(_productRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new DeleteDosageFormCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenDosageFormNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _dosageFormRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((DosageForm?)null);

        var result = await _handler.Handle(new DeleteDosageFormCommand(1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DosageFormErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenDosageFormIsUsed_ReturnsIsUsedError()
    {
        var dosageForm = DosageForm.Create("Tablet");
        dosageForm.Id = 1;

        A.CallTo(() => _dosageFormRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(dosageForm);

        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var result = await _handler.Handle(new DeleteDosageFormCommand(1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DosageFormErrors.IsUsed, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var dosageForm = DosageForm.Create("Tablet");
        dosageForm.Id = 1;

        A.CallTo(() => _dosageFormRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(dosageForm);

        A.CallTo(() => _productRepo.AnyAsync(A<Expression<Func<Product, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new DeleteDosageFormCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);

        A.CallTo(() => _dosageFormRepo.Delete(A<DosageForm>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
