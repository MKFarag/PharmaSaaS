namespace PharmaSaaS.Application.Features.Expenses.Delete;

public sealed class DeleteExpenseCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteExpenseCommand, Result>
{
    public async Task<Result> Handle(DeleteExpenseCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Expenses.GetAsync([command.Id], cancellationToken) is not { } expense)
            return Result.Failure(ExpenseErrors.NotFound);

        if (await unitOfWork.Shifts.GetAsync([expense.ShiftId], cancellationToken) is not { } shift)
            return Result.Failure(ShiftErrors.NotFound);

        if (!shift.IsOpen)
            return Result.Failure(ShiftErrors.NotOpen);

        shift.RemoveExpense(expense.Amount);

        unitOfWork.Expenses.Delete(expense);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
