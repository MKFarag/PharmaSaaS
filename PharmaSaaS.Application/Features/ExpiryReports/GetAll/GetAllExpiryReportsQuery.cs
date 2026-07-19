namespace PharmaSaaS.Application.Features.ExpiryReports.GetAll;

public record GetAllExpiryReportsQuery(ExpiryReportRequestFilters Filters) : IRequest<IPaginatedList<ExpiryReportResponse>>;
