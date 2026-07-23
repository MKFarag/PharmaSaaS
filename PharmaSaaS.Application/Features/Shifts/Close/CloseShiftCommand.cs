namespace PharmaSaaS.Application.Features.Shifts.Close;

public record CloseShiftCommand(int ShiftId, CloseShiftRequest Request) : IRequest<Result<CloseShiftResponse>>;
