namespace PharmaSaaS.Application.Contracts.Expenses;

public record ExpenseResponse(
    int Id,
    decimal Amount,
    string Description,
    DateTimeOffset Date,
    int ShiftId
);
