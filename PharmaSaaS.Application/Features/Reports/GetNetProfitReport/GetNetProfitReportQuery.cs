namespace PharmaSaaS.Application.Features.Reports.GetNetProfitReport;

public record GetNetProfitReportQuery(DateOnly From, DateOnly To) : IRequest<Result<NetProfitReportResponse>>;
