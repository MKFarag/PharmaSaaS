namespace PharmaSaaS.Application.Features.Reports.GetTopProductsReport;

public sealed class GetTopProductsReportQueryHandler(IReportService reportService) : IRequestHandler<GetTopProductsReportQuery, Result<IEnumerable<TopProductReportResponse>>>
{
    public async Task<Result<IEnumerable<TopProductReportResponse>>> Handle(GetTopProductsReportQuery request, CancellationToken cancellationToken = default)
    {
        if (request.From > request.To)
            return Result.Failure<IEnumerable<TopProductReportResponse>>(ReportErrors.InvalidDateRange);

        if (request.Limit < 1)
            return Result.Failure<IEnumerable<TopProductReportResponse>>(ReportErrors.InvalidLimit);

        return Result.Success(await reportService.GetTopSellingProductsAsync(request.From, request.To, request.Limit, cancellationToken));
    }
}
