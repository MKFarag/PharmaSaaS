namespace PharmaSaaS.Application.Features.Users.Get;

public sealed class GetUserQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetUserQuery, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByIdAsync(request.Id, cancellationToken) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.NotFound);

        var roles = await unitOfWork.Users.GetRolesAsync(user);

        var response = (user, roles).Adapt<UserResponse>();

        return Result.Success(response);
    }
}
