namespace PharmaSaaS.Application.Features.Auth.GetRefreshToken;

public class GetRefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtProvider jwtProvider) : IRequestHandler<GetRefreshTokenCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(GetRefreshTokenCommand request, CancellationToken cancellationToken = default)
    {
        if (jwtProvider.ValidateToken(request.Token) is not { } userId)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        if (await unitOfWork.Users.FindByIdAsync(userId, cancellationToken) is not { } user)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        if (user.IsDisabled)
            return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

        if (await unitOfWork.Users.IsLockedOutAsync(user))
            return Result.Failure<AuthResponse>(UserErrors.LockedUser);

        var revokeResult = await unitOfWork.Users.RevokeRefreshTokenAsync(user, request.RefreshToken, cancellationToken);

        if (revokeResult.IsFailure)
            return Result.Failure<AuthResponse>(revokeResult.Error);

        var (userRoles, userPermissions) = await unitOfWork.Users.GetRolesAndPermissionsAsync(user, cancellationToken);

        var (newToken, expiresIn) = jwtProvider.GenerateToken(user, userRoles, userPermissions);
        var newRefreshToken = RefreshToken.Create();

        await unitOfWork.Users.AddRefreshTokenAsync(user, newRefreshToken, cancellationToken);

        var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, newToken, expiresIn, newRefreshToken.Token, newRefreshToken.ExpiresOn);

        return Result.Success(response);
    }
}

