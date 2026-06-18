namespace PharmaSaaS.Application.Features.Auth.GetRefreshToken;

public record GetRefreshTokenCommand(string Token, string RefreshToken) : IRequest<Result<AuthResponse>>;
