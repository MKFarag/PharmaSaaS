namespace PharmaSaaS.Application.Features.Users.Update;

public sealed class UpdateUserCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByIdAsync(command.Id, cancellationToken) is not { } user)
            return Result.Failure(UserErrors.NotFound);

        if (!string.Equals(user.Email, command.Request.Email, StringComparison.OrdinalIgnoreCase) &&
            await unitOfWork.Users.EmailExistsAsync(command.Request.Email, command.Id, cancellationToken))
            return Result.Failure(UserErrors.DuplicatedEmail);

        var allowedRoles = await unitOfWork.Roles.GetAllNamesAsync(false, false, cancellationToken);

        if (command.Request.Roles.Except(allowedRoles).Any())
            return Result.Failure(UserErrors.InvalidRoles);

        user.Update(command.Request.Email, command.Request.FirstName, command.Request.LastName);

        var result = await unitOfWork.Users.UpdateAsync(user);

        if (result.IsFailure)
            return result;

        var currentRoles = await unitOfWork.Users.GetRolesAsync(user);
        var rolesToAdd = command.Request.Roles.Except(currentRoles);
        var rolesToRemove = currentRoles.Except(command.Request.Roles);

        if (rolesToAdd.Any() || rolesToRemove.Any())
        {
            await unitOfWork.Users.DeleteAllRolesAsync(user);
            await unitOfWork.Users.AddToRolesAsync(user, command.Request.Roles);
        }

        await cacheService.RemoveByTagAsync(Cache.Tags.Users(currentTenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success();
    }
}
