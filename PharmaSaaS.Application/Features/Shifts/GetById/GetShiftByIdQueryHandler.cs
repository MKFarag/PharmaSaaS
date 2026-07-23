namespace PharmaSaaS.Application.Features.Shifts.GetById;

public sealed class GetShiftByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetShiftByIdQuery, Result<ShiftDetailResponse>>
{
    public async Task<Result<ShiftDetailResponse>> Handle(GetShiftByIdQuery request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Shifts.GetAsync([request.Id], cancellationToken) is not { } shift)
            return Result.Failure<ShiftDetailResponse>(ShiftErrors.NotFound);

        return Result.Success(shift.Adapt<ShiftDetailResponse>());
    }
}
