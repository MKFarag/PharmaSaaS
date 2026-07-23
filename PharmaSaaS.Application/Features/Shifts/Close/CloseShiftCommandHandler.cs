namespace PharmaSaaS.Application.Features.Shifts.Close;

public sealed class CloseShiftCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CloseShiftCommand, Result<CloseShiftResponse>>
{
    public async Task<Result<CloseShiftResponse>> Handle(CloseShiftCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Shifts.GetAsync([command.ShiftId], cancellationToken) is not { } shift)
            return Result.Failure<CloseShiftResponse>(ShiftErrors.NotFound);

        if (shift.Status != ShiftStatus.Open)
            return Result.Failure<CloseShiftResponse>(ShiftErrors.NotOpen);

        shift.Close(command.Request.ActualBalance);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(new CloseShiftResponse(shift.ExpectedBalance!.Value, shift.Discrepancy!.Value));
    }
}
