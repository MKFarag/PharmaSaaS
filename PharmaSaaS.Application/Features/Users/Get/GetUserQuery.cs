namespace PharmaSaaS.Application.Features.Users.Get;

public record GetUserQuery(string Id) : IRequest<Result<UserResponse>>;
