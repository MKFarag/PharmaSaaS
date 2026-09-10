namespace PharmaSaaS.Application.Features.Expenses.GetAllByShift;

public sealed class GetAllExpensesByShiftQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllExpensesByShiftQuery, Result<IEnumerable<ExpenseResponse>>>
{
    public async Task<Result<IEnumerable<ExpenseResponse>>> Handle(GetAllExpensesByShiftQuery query, CancellationToken cancellationToken = default)
    {
        var expenses = await unitOfWork.Expenses.FindAllProjectionAsync<ExpenseResponse>(x => x.ShiftId == query.ShiftId, cancellationToken);

        return Result.Success(expenses);
    }
}
