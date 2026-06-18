namespace PharmaSaaS.Application.Features.Auth.GetToken;

public record GetTokenCommand(string Identifier, string Password) : IRequest<Result<AuthResponse>>;
