namespace PharmaSaaS.Application.Features.Roles.GetAll;

public record GetAllRolesQuery(bool IncludeDisabled) : IRequest<IEnumerable<RoleResponse>>;
