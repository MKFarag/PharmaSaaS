namespace PharmaSaaS.Application.Features.Reports.GetShortageReport;

public record GetShortageReportQuery() : IRequest<IEnumerable<ShortageReportResponse>>;
