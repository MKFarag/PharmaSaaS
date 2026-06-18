namespace PharmaSaaS.Application.Features.Users.Add;

public record AddUserCommand(CreateUserRequest Request) : IRequest<Result<UserResponse>>;
