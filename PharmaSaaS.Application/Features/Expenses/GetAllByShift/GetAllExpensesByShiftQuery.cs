namespace PharmaSaaS.Application.Features.Expenses.GetAllByShift;

public record GetAllExpensesByShiftQuery(int ShiftId) : IRequest<IEnumerable<ExpenseResponse>>;
