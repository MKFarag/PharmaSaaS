namespace PharmaSaaS.Application.Features.Auth.RevokeRefreshToken;

public record RevokeRefreshTokenCommand(string Token, string RefreshToken) : IRequest<Result>;
