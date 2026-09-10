namespace PharmaSaaS.Application.Features.Shifts.GetAll;

public sealed class GetAllShiftsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllShiftsQuery, IPaginatedList<ShiftResponse>>
{
    public async Task<IPaginatedList<ShiftResponse>> Handle(GetAllShiftsQuery request, CancellationToken cancellationToken = default)
    {
        return await unitOfWork.Shifts
            .FindPaginatedListAsync<ShiftResponse>
            (
                s => !request.Filters.Date.HasValue || s.Date == request.Filters.Date,
                request.Filters.PageNumber, request.Filters.PageSize,
                nameof(Shift.Id),
                cancellationToken
            );
    }
}
