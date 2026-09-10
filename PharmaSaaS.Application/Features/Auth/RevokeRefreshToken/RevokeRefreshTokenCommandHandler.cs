namespace PharmaSaaS.Application.Features.Auth.RevokeRefreshToken;

public sealed class RevokeRefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtProvider jwtProvider) : IRequestHandler<RevokeRefreshTokenCommand, Result>
{
    public async Task<Result> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken = default)
    {
        if (jwtProvider.ValidateToken(request.Token) is not { } userId)
            return Result.Failure(UserErrors.InvalidJwtToken);

        if (await unitOfWork.Users.FindByIdAsync(userId, cancellationToken) is not { } user)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var revokeResult = await unitOfWork.Users.RevokeRefreshTokenAsync(user, request.RefreshToken, cancellationToken);

        if (revokeResult.IsFailure)
            return Result.Failure(revokeResult.Error);

        return Result.Success();
    }
}
