namespace PharmaSaaS.Application.Interfaces.Infrastructure;

public interface IJwtProvider
{
    (string token, int expiresIn) GenerateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string? ValidateToken(string token);
}
