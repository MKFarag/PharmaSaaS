using PharmaSaaS.Application.Abstractions.Messaging;
using PharmaSaaS.Application.Features.Expenses.Add;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Constants;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Application.Features.Expenses.Add;

public sealed class LogExpenseCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<LogExpenseCommand, Result<int>>
{
    public async Task<Result<int>> Handle(LogExpenseCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Shifts.TrackedFindAsync(s => s.Status == ShiftStatus.Open, cancellationToken) is not { } shift)
            return Result.Failure<int>(ShiftErrors.NotOpen);

        var expense = Expense.Create(shift.Id, command.Request.Amount, command.Request.Description);

        shift.AddExpense(expense.Amount);
        
        await unitOfWork.Expenses.AddAsync(expense, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(expense.Id);
    }
}
