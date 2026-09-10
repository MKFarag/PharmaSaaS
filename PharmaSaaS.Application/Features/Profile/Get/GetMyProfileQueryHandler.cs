namespace PharmaSaaS.Application.Features.Profile.Get;

public sealed class GetMyProfileQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMyProfileQuery, Result<UserProfileResponse>>
{
    public async Task<Result<UserProfileResponse>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByIdAsync(request.UserId, cancellationToken) is not { } user)
            return Result.Failure<UserProfileResponse>(UserErrors.NotFound);

        var response = user.Adapt<UserProfileResponse>();

        return Result.Success(response);
    }
}
