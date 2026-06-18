namespace PharmaSaaS.Application.Features.Profile.ConfirmChangeEmail;

public class ConfirmChangeMyEmailCommandHandler(IUnitOfWork unitOfWork, IUrlEncoder urlEncoder, INotificationService notificationService) : IRequestHandler<ConfirmChangeMyEmailCommand, Result>
{
    public async Task<Result> Handle(ConfirmChangeMyEmailCommand request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByIdAsync(request.UserId, cancellationToken) is not { } user)
            return Result.Failure(UserErrors.NotFound);

        var oldEmail = user.Email;

        if (urlEncoder.Decode(request.Token) is not { } token)
            return Result.Failure(UserErrors.InvalidToken);

        var result = await unitOfWork.Users.ChangeEmailAsync(user, request.NewEmail, token);

        if (result.IsSuccess)
            await notificationService.SendChangeEmailNotificationAsync(user, oldEmail, DateTime.UtcNow);

        return result;
    }
}
