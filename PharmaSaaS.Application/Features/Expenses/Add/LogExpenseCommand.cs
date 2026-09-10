namespace PharmaSaaS.Application.Features.Expenses.Add;

public record LogExpenseCommand(ExpenseRequest Request) : IRequest<Result<ExpenseResponse>>;
