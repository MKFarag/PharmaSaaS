namespace PharmaSaaS.Application.Features.Profile.Update;

public record UpdateProfileCommand(string UserId, UpdateUserProfileRequest Request) : IRequest<Result>;
