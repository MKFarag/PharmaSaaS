namespace PharmaSaaS.Application.Features.Expenses.Delete;

public record DeleteExpenseCommand(int Id) : IRequest<Result>;
