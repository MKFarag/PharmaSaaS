using PharmaSaaS.Application.Features.DosageForms.Add;

namespace PharmaSaaS.Application.Test.DosageForms;

public class AddDosageFormCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<DosageForm> _dosageFormRepo = A.Fake<IGenericRepository<DosageForm>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly AddDosageFormCommandHandler _handler;

    public AddDosageFormCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.DosageForms).Returns(_dosageFormRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new AddDosageFormCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenNameDuplicated_ReturnsDuplicatedNameError()
    {
        A.CallTo(() => _dosageFormRepo.AnyAsync(A<Expression<Func<DosageForm, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new AddDosageFormCommand(new DosageFormRequest("Tablet"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DosageFormErrors.DuplicatedName, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        A.CallTo(() => _dosageFormRepo.AnyAsync(A<Expression<Func<DosageForm, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new AddDosageFormCommand(new DosageFormRequest("Tablet"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Tablet", result.Value.Name);

        A.CallTo(() => _dosageFormRepo.AddAsync(A<DosageForm>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
