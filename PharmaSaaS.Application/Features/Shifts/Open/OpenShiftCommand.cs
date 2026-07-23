namespace PharmaSaaS.Application.Features.Shifts.Open;

public record OpenShiftCommand(OpenShiftRequest Request, string UserId) : IRequest<Result<ShiftResponse>>;
