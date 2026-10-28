namespace PharmaSaaS.Application.Features.Roles.GetAllPermissions;

public record GetAllPermissionsQuery : IRequest<IEnumerable<PermissionResponse>>;
