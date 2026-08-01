using Mapster;
using PharmaSaaS.Application.Abstractions.Messaging;
using PharmaSaaS.Application.Contracts.Expenses;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;

namespace PharmaSaaS.Application.Features.Expenses.GetAllByShift;

public sealed class GetAllExpensesByShiftQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllExpensesByShiftQuery, Result<List<ExpenseResponse>>>
{
    public async Task<Result<List<ExpenseResponse>>> Handle(GetAllExpensesByShiftQuery query, CancellationToken cancellationToken = default)
    {
        var expenses = await unitOfWork.Expenses.FindAllAsync(e => e.ShiftId == query.ShiftId, cancellationToken: cancellationToken);

        return Result.Success(expenses.Adapt<List<ExpenseResponse>>());
    }
}
