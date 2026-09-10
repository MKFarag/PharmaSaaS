namespace PharmaSaaS.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IUrlEncoder urlEncoder) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken) is not { } user ||
            !await unitOfWork.Users.IsEmailConfirmedAsync(user))
            return Result.Failure(UserErrors.InvalidToken);

        if (urlEncoder.Decode(request.Token) is not { } token)
            return Result.Failure(UserErrors.InvalidToken);

        var result = await unitOfWork.Users.ResetPasswordAsync(user, token, request.NewPassword);

        return result;
    }
}
