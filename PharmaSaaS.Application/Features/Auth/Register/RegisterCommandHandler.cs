namespace PharmaSaaS.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler(IUnitOfWork unitOfWork, IUrlEncoder urlEncoder, INotificationService notificationService) : IRequestHandler<RegisterCommand, Result>
{
    public async Task<Result> Handle(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.EmailExistsAsync(command.Request.Email, cancellationToken))
            return Result.Failure(UserErrors.DuplicatedEmail);

        var user = User.Create(command.Request.Email, command.Request.FirstName, command.Request.LastName);

        var result = await unitOfWork.Users.CreateAsync(user, command.Request.Password);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        var token = await unitOfWork.Users.GenerateEmailConfirmationTokenAsync(user);
        token = urlEncoder.Encode(token);

        await notificationService.SendConfirmationLinkAsync(user, token);

        return Result.Success();
    }

}
