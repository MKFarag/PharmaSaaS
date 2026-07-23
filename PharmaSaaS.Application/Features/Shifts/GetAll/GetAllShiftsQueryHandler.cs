namespace PharmaSaaS.Application.Features.Shifts.GetAll;

public sealed class GetAllShiftsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllShiftsQuery, IPaginatedList<ShiftResponse>>
{
    public async Task<IPaginatedList<ShiftResponse>> Handle(GetAllShiftsQuery request, CancellationToken cancellationToken = default)
    {
        var f = request.Filters;

        if (request.Filters.Date is { } date)
        {
            return await unitOfWork.Shifts.FindPaginatedListAsync<ShiftResponse>(
                s => s.Date == date,
                f.PageNumber, f.PageSize,
                nameof(Shift.Id),
                cancellationToken);
        }

        return await unitOfWork.Shifts.GetPaginatedListAsync<ShiftResponse>(
            f.PageNumber, f.PageSize,
            nameof(Shift.Id),
            cancellationToken);
    }
}
