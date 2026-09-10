namespace PharmaSaaS.Application.Features.Expenses.Add;

public sealed class LogExpenseCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<LogExpenseCommand, Result<ExpenseResponse>>
{
    public async Task<Result<ExpenseResponse>> Handle(LogExpenseCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Shifts.TrackedFindAsync(s => s.Status == ShiftStatus.Open, cancellationToken) is not { } shift)
            return Result.Failure<ExpenseResponse>(ShiftErrors.NotOpen);

        var expense = Expense.Create(shift.Id, command.Request.Amount, command.Request.Description);

        shift.AddExpense(expense.Amount);

        await unitOfWork.Expenses.AddAsync(expense, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(expense.Adapt<ExpenseResponse>());
    }
}
