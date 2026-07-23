namespace PharmaSaaS.Application.Features.Shifts.Open;

public sealed class OpenShiftCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<OpenShiftCommand, Result<ShiftResponse>>
{
    public async Task<Result<ShiftResponse>> Handle(OpenShiftCommand command, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (await unitOfWork.Shifts.AnyAsync(s => s.Date == today && s.Status == ShiftStatus.Open, cancellationToken))
            return Result.Failure<ShiftResponse>(ShiftErrors.AlreadyOpen);

        var shift = Shift.Create(command.UserId, command.Request.OpeningBalance);

        await unitOfWork.Shifts.AddAsync(shift, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(shift.Adapt<ShiftResponse>());
    }
}
