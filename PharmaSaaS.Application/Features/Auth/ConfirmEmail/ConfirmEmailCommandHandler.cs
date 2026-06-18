namespace PharmaSaaS.Application.Features.Auth.ConfirmEmail;

public class ConfirmEmailCommandHandler(IUnitOfWork unitOfWork, IUrlEncoder urlEncoder) : IRequestHandler<ConfirmEmailCommand, Result>
{
    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByIdAsync(request.UserId, cancellationToken) is not { } user)
            return Result.Failure(UserErrors.InvalidToken);

        if (await unitOfWork.Users.IsEmailConfirmedAsync(user))
            return Result.Failure(UserErrors.DuplicatedConfirmation);

        if (urlEncoder.Decode(request.Token) is not { } token)
            return Result.Failure(UserErrors.InvalidToken);

        var result = await unitOfWork.Users.ConfirmEmailWithTokenAsync(user, token);

        if (result.IsSuccess)
            result = await unitOfWork.Users.AddToRoleAsync(user, DefaultRoles.User.Name);

        return result;
    }
}
