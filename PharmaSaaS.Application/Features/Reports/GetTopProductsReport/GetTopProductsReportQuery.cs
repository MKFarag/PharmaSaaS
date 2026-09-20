namespace PharmaSaaS.Application.Features.Reports.GetTopProductsReport;

public record GetTopProductsReportQuery(DateOnly From, DateOnly To, int Limit) : IRequest<Result<IEnumerable<TopProductReportResponse>>>;
