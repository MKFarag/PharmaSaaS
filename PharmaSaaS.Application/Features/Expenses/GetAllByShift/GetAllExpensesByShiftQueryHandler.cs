namespace PharmaSaaS.Application.Features.Expenses.GetAllByShift;

public sealed class GetAllExpensesByShiftQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllExpensesByShiftQuery, IEnumerable<ExpenseResponse>>
{
    public async Task<IEnumerable<ExpenseResponse>> Handle(GetAllExpensesByShiftQuery query, CancellationToken cancellationToken = default)
        => await unitOfWork.Expenses.FindAllProjectionAsync<ExpenseResponse>(x => x.ShiftId == query.ShiftId, cancellationToken);
}
