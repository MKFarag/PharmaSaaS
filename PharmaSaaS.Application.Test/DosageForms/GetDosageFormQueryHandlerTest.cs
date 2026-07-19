using PharmaSaaS.Application.Features.DosageForms.Get;

namespace PharmaSaaS.Application.Test.DosageForms;

public class GetDosageFormQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<DosageForm> _dosageFormRepo = A.Fake<IGenericRepository<DosageForm>>();
    private readonly GetDosageFormQueryHandler _handler;

    public GetDosageFormQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.DosageForms).Returns(_dosageFormRepo);
        _handler = new GetDosageFormQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenDosageFormFound_ReturnsSuccess()
    {
        var dosageForm = DosageForm.Create("Tablet");
        dosageForm.Id = 1;

        A.CallTo(() => _dosageFormRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(dosageForm);

        var result = await _handler.Handle(new GetDosageFormQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Tablet", result.Value.Name);
    }

    [Fact]
    public async Task Handle_WhenDosageFormNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _dosageFormRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((DosageForm?)null);

        var result = await _handler.Handle(new GetDosageFormQuery(1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DosageFormErrors.NotFound, result.Error);
    }
}
