namespace PharmaSaaS.Application.Contracts.Reports;

public record ExpenseReportResponse(
    int Id,
    decimal Amount,
    string Description,
    DateTimeOffset Date,
    int ShiftId,
    DateOnly ShiftDate
);
