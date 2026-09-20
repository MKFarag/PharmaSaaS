namespace PharmaSaaS.Application.Features.Reports.GetExpenseReport;

public sealed class GetExpenseReportQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetExpenseReportQuery, Result<IEnumerable<ExpenseReportResponse>>>
{
    public async Task<Result<IEnumerable<ExpenseReportResponse>>> Handle(GetExpenseReportQuery request, CancellationToken cancellationToken = default)
    {
        if (request.From > request.To)
            return Result.Failure<IEnumerable<ExpenseReportResponse>>(ReportErrors.InvalidDateRange);

        var from = new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue));
        var to = new DateTimeOffset(request.To.ToDateTime(TimeOnly.MaxValue));

        var expenses = await unitOfWork.Expenses.FindAllAsync(
            e => e.Date >= from && e.Date <= to,
            [nameof(Expense.Shift)],
            cancellationToken);

        var responses = expenses.Select(e => new ExpenseReportResponse(e.Id, e.Amount, e.Description, e.Date, e.ShiftId, e.Shift.Date));

        return Result.Success(responses);
    }
}
