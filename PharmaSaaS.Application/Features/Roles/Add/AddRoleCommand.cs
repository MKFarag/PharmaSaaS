namespace PharmaSaaS.Application.Features.Roles.Add;

public record AddRoleCommand(RoleRequest Request) : IRequest<Result<RoleDetailResponse>>;
