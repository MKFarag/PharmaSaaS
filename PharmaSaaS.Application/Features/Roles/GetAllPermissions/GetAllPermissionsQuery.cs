using PharmaSaaS.Application.Contracts.Roles;

namespace PharmaSaaS.Application.Features.Roles.GetAllPermissions;

public record GetAllPermissionsQuery : IRequest<IEnumerable<PermissionResponse>>;
