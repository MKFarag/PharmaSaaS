namespace PharmaSaaS.Application.Features.Reports.GetDailyReport;

public record GetDailyReportQuery(DateOnly Date) : IRequest<DailyReportResponse>;
