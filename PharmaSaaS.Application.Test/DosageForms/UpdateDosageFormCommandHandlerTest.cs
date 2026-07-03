using PharmaSaaS.Application.Contracts.DosageForms;
using PharmaSaaS.Application.Features.DosageForms.Update;

namespace PharmaSaaS.Application.Test.DosageForms;

public class UpdateDosageFormCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<DosageForm> _dosageFormRepo = A.Fake<IGenericRepository<DosageForm>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly UpdateDosageFormCommandHandler _handler;

    public UpdateDosageFormCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.DosageForms).Returns(_dosageFormRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new UpdateDosageFormCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenDosageFormNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _dosageFormRepo.FindAsync(A<Expression<Func<DosageForm, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns((DosageForm?)null);

        var command = new UpdateDosageFormCommand(1, new DosageFormRequest("New Name"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DosageFormErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenNameDuplicated_ReturnsDuplicatedNameError()
    {
        var dosageForm = DosageForm.Create("Old Name");
        dosageForm.Id = 1;

        A.CallTo(() => _dosageFormRepo.FindAsync(A<Expression<Func<DosageForm, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(dosageForm);

        A.CallTo(() => _dosageFormRepo.AnyAsync(A<Expression<Func<DosageForm, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new UpdateDosageFormCommand(1, new DosageFormRequest("Existing Name"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DosageFormErrors.DuplicatedName, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var dosageForm = DosageForm.Create("Old Name");
        dosageForm.Id = 1;

        A.CallTo(() => _dosageFormRepo.FindAsync(A<Expression<Func<DosageForm, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(dosageForm);

        A.CallTo(() => _dosageFormRepo.AnyAsync(A<Expression<Func<DosageForm, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new UpdateDosageFormCommand(1, new DosageFormRequest("New Name"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
