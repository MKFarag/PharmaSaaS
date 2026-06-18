namespace PharmaSaaS.Application.Contracts.Profile;

public record UserProfileResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email
);
