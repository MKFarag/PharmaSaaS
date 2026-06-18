namespace PharmaSaaS.Application.Features.Roles.Get;

public record GetRoleQuery(string Id) : IRequest<Result<RoleDetailResponse>>;
