namespace PharmaSaaS.Application.Features.Auth.GetToken;

public class GetTokenCommandHandler(IUnitOfWork unitOfWork, IJwtProvider jwtProvider, ISignInService signInService) : IRequestHandler<GetTokenCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(GetTokenCommand request, CancellationToken cancellationToken = default)
    {
        var result = await signInService.PasswordSignInAsync(request.Identifier, request.Password, true);

        if (result.IsFailure)
            return Result.Failure<AuthResponse>(result.Error);

        var user = result.Value;

        if (user.IsDisabled)
            return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

        var (userRoles, userPermissions) = await unitOfWork.Users.GetRolesAndPermissionsAsync(user, cancellationToken);

        var (token, expiresIn) = jwtProvider.GenerateToken(user, userRoles, userPermissions);

        var refreshToken = RefreshToken.Create();

        await unitOfWork.Users.AddRefreshTokenAsync(user, refreshToken, cancellationToken);

        var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn, refreshToken.Token, refreshToken.ExpiresOn);

        return Result.Success(response);
    }
}
