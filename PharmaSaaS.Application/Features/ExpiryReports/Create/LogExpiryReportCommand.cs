namespace PharmaSaaS.Application.Features.ExpiryReports.Create;

public record LogExpiryReportCommand(ExpiryReportRequest Request, string UserId) : IRequest<Result<ExpiryReportResponse>>;
