namespace PharmaSaaS.Application.Features.Users.Update;

public record UpdateUserCommand(string Id, UpdateUserRequest Request) : IRequest<Result>;
