using PharmaSaaS.Application.Contracts.Expenses;
using PharmaSaaS.Domain.Abstraction;

namespace PharmaSaaS.Application.Features.Expenses.Add;

public record LogExpenseCommand(ExpenseRequest Request) : IRequest<Result<int>>;
