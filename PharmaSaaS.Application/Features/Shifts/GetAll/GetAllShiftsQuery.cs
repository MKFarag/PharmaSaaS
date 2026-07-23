namespace PharmaSaaS.Application.Features.Shifts.GetAll;

public record GetAllShiftsQuery(ShiftRequestFilters Filters) : IRequest<IPaginatedList<ShiftResponse>>;
