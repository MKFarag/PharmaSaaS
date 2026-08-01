using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Application.Abstractions.Messaging;

namespace PharmaSaaS.Application.Features.Expenses.Delete;

public record DeleteExpenseCommand(int Id) : IRequest<Result>;
