namespace PharmaSaaS.Application.Features.Reports.GetExpenseReport;

public record GetExpenseReportQuery(DateOnly From, DateOnly To) : IRequest<Result<IEnumerable<ExpenseReportResponse>>>;
