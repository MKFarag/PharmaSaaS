namespace PharmaSaaS.Application.Features.Reports.GetDailyReport;

public sealed class GetDailyReportQueryHandler(IReportService reportService) : IRequestHandler<GetDailyReportQuery, DailyReportResponse>
{
    public async Task<DailyReportResponse> Handle(GetDailyReportQuery request, CancellationToken cancellationToken = default)
        => await reportService.GetDailyReportAsync(request.Date, cancellationToken);
}
