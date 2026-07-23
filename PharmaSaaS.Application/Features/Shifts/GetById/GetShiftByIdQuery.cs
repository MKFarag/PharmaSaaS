namespace PharmaSaaS.Application.Features.Shifts.GetById;

public record GetShiftByIdQuery(int Id) : IRequest<Result<ShiftDetailResponse>>;
