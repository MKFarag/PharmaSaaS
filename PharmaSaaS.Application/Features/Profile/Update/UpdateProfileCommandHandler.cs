namespace PharmaSaaS.Application.Features.Profile.Update;

public class UpdateProfileCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateProfileCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByIdAsync(command.UserId, cancellationToken) is not { } user)
            return Result.Failure(UserErrors.NotFound);

        user.ChangeName(command.Request.FirstName, command.Request.LastName);

        await unitOfWork.Users.UpdateAsync(user);

        return Result.Success();
    }
}
