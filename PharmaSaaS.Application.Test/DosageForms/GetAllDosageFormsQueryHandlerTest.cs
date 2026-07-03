using PharmaSaaS.Application.Features.DosageForms.GetAll;

namespace PharmaSaaS.Application.Test.DosageForms;

public class GetAllDosageFormsQueryHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepository<DosageForm> _dosageFormRepo = A.Fake<IGenericRepository<DosageForm>>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly GetAllDosageFormsQueryHandler _handler;

    public GetAllDosageFormsQueryHandlerTest()
    {
        A.CallTo(() => _unitOfWork.DosageForms).Returns(_dosageFormRepo);
        A.CallTo(() => _currentTenantGetter.GetTenantId()).Returns("tenant-1");
        _handler = new GetAllDosageFormsQueryHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_ReturnsAllDosageForms()
    {
        var dosageForms = new List<DosageFormResponse>
        {
            new(1, "Tablet"),
            new(2, "Capsule")
        };

        A.CallTo(() => _cacheService.GetOrCreateAsync(
                A<string>.Ignored,
                A<Func<CancellationToken, Task<IEnumerable<DosageFormResponse>>>>.Ignored,
                A<TimeSpan?>.Ignored,
                A<IEnumerable<string>?>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(dosageForms);

        var result = await _handler.Handle(new GetAllDosageFormsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, d => d.Name == "Tablet");
    }
}
