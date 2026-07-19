namespace PharmaSaaS.Application.Test.ExpiryReportsTests;

public class GetAllExpiryReportsQueryHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<ExpiryReport> _repo = A.Fake<IGenericRepositoryWithPagination<ExpiryReport>>();
    private readonly GetAllExpiryReportsQueryHandler _handler;

    public GetAllExpiryReportsQueryHandlerTest()
    {
        A.CallTo(() => _unitOfWork.ExpiryReports).Returns(_repo);
        _handler = new GetAllExpiryReportsQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_ReturnsPagedExpiryReports()
    {
        var paged = A.Fake<IPaginatedList<ExpiryReportResponse>>();

        A.CallTo(() => _repo.FindPaginatedListAsync<ExpiryReportResponse>(
                A<Expression<Func<ExpiryReport, bool>>>.Ignored,
                A<int>.Ignored, A<int>.Ignored,
                A<string?>.Ignored, A<string>.Ignored,
                A<string>.Ignored!, A<string>.Ignored!,
                A<ColumnType>.Ignored,
                A<string[]>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(paged);

        var result = await _handler.Handle(
            new GetAllExpiryReportsQuery(new ExpiryReportRequestFilters()),
            CancellationToken.None);

        Assert.Equal(paged, result);

        A.CallTo(() => _repo.FindPaginatedListAsync<ExpiryReportResponse>(
                A<Expression<Func<ExpiryReport, bool>>>.Ignored,
                A<int>.Ignored, A<int>.Ignored,
                A<string?>.Ignored, A<string>.Ignored,
                A<string>.Ignored!, A<string>.Ignored!,
                A<ColumnType>.Ignored,
                A<string[]>.Ignored,
                A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
