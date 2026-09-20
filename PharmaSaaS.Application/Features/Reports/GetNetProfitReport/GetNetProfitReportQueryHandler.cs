namespace PharmaSaaS.Application.Features.Reports.GetNetProfitReport;

public sealed class GetNetProfitReportQueryHandler(IReportService reportService) : IRequestHandler<GetNetProfitReportQuery, Result<NetProfitReportResponse>>
{
    public async Task<Result<NetProfitReportResponse>> Handle(GetNetProfitReportQuery request, CancellationToken cancellationToken = default)
    {
        if (request.From > request.To)
            return Result.Failure<NetProfitReportResponse>(ReportErrors.InvalidDateRange);

        return Result.Success(await reportService.GetNetProfitReportAsync(request.From, request.To, cancellationToken));
    }
}
