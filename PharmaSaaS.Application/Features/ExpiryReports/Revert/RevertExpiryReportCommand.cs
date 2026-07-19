namespace PharmaSaaS.Application.Features.ExpiryReports.Revert;

public record RevertExpiryReportCommand(int Id) : IRequest<Result>;
