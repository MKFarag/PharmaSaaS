using PharmaSaaS.Application.Contracts.Expenses;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Application.Abstractions.Messaging;

namespace PharmaSaaS.Application.Features.Expenses.GetAllByShift;

public record GetAllExpensesByShiftQuery(int ShiftId) : IRequest<Result<List<ExpenseResponse>>>;
