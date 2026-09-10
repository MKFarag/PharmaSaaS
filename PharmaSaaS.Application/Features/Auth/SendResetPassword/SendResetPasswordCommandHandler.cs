namespace PharmaSaaS.Application.Features.Auth.SendResetPassword;

public sealed class SendResetPasswordCommandHandler(IUnitOfWork unitOfWork, IUrlEncoder urlEncoder, INotificationService notificationService) : IRequestHandler<SendResetPasswordCommand, Result>
{
    public async Task<Result> Handle(SendResetPasswordCommand request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken) is not { } user)
            return Result.Success();

        if (!await unitOfWork.Users.IsEmailConfirmedAsync(user))
            return Result.Failure(UserErrors.EmailNotConfirmed with { StatusCode = StatusCodes.BadRequest });

        var token = await unitOfWork.Users.GeneratePasswordResetTokenAsync(user);
        token = urlEncoder.Encode(token);

        await notificationService.SendResetPasswordAsync(user, token);

        return Result.Success();
    }
}
