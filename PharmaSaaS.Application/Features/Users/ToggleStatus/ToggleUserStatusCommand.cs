namespace PharmaSaaS.Application.Features.Users.ToggleStatus;

public record ToggleUserStatusCommand(string Id) : IRequest<Result>;
