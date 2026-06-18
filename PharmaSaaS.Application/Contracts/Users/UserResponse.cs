namespace PharmaSaaS.Application.Contracts.Users;

public record UserResponse(
    string Id,
    string FullName,
    string Email,
    bool IsDisabled,
    IEnumerable<string> Roles
);
