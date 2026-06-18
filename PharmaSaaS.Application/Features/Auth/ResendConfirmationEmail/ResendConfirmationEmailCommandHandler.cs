namespace PharmaSaaS.Application.Features.Auth.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandHandler(IUnitOfWork unitOfWork, IUrlEncoder urlEncoder, INotificationService notificationService) : IRequestHandler<ResendConfirmationEmailCommand, Result>
{
    public async Task<Result> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken) is not { } user)
            return Result.Success();

        if (await unitOfWork.Users.IsEmailConfirmedAsync(user))
            return Result.Failure(UserErrors.DuplicatedConfirmation);

        var token = await unitOfWork.Users.GenerateEmailConfirmationTokenAsync(user);
        token = urlEncoder.Encode(token);

        await notificationService.SendConfirmationLinkAsync(user, token);

        return Result.Success();
    }
}
