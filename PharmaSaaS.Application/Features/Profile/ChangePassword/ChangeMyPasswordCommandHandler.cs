namespace PharmaSaaS.Application.Features.Profile.ChangePassword;

public sealed class ChangeMyPasswordCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ChangeMyPasswordCommand, Result>
{
    public async Task<Result> Handle(ChangeMyPasswordCommand request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByIdAsync(request.UserId, cancellationToken) is not { } user)
            return Result.Failure(UserErrors.NotFound);

        var result = await unitOfWork.Users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        return result;
    }
}
