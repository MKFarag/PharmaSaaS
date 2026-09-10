namespace PharmaSaaS.Application.Features.Profile.ChangeEmail;

public sealed class ChangeMyEmailCommandHandler(IUnitOfWork unitOfWork, IUrlEncoder urlEncoder, INotificationService notificationService) : IRequestHandler<ChangeMyEmailCommand, Result>
{
    public async Task<Result> Handle(ChangeMyEmailCommand request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByIdAsync(request.UserId, cancellationToken) is not { } user)
            return Result.Failure(UserErrors.NotFound);

        if (string.Equals(user.Email, request.NewEmail, StringComparison.OrdinalIgnoreCase))
            return Result.Failure(UserErrors.SameEmail);

        if (await unitOfWork.Users.EmailExistsAsync(request.NewEmail, cancellationToken))
            return Result.Failure(UserErrors.DuplicatedEmail);

        var token = await unitOfWork.Users.GenerateChangeEmailTokenAsync(user, request.NewEmail);

        token = urlEncoder.Encode(token);

        await notificationService.SendConfirmationLinkAsync(user, token);

        return Result.Success();
    }
}
