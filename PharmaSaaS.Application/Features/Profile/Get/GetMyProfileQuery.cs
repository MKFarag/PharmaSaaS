namespace PharmaSaaS.Application.Features.Profile.Get;

public record GetMyProfileQuery(string UserId) : IRequest<Result<UserProfileResponse>>;