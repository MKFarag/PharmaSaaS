namespace PharmaSaaS.Application.Features.Roles.ToggleStatus;

public record ToggleRoleStatusCommand(string Id) : IRequest<Result>;
