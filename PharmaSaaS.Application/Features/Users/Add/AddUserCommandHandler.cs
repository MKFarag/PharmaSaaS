namespace PharmaSaaS.Application.Features.Users.Add;

public class AddUserCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<AddUserCommand, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> Handle(AddUserCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.EmailExistsAsync(command.Request.Email, cancellationToken))
            return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);

        var allowedRoles = await unitOfWork.Roles.GetAllNamesAsync(false, false, cancellationToken);

        if (command.Request.Roles.Except(allowedRoles).Any())
            return Result.Failure<UserResponse>(UserErrors.InvalidRoles);

        var user = User.Create(command.Request.Email, command.Request.FirstName, command.Request.LastName);

        var result = await unitOfWork.Users.CreateAsync(user, command.Request.Password, true);

        if (result.IsFailure)
            return Result.Failure<UserResponse>(result.Error);

        await unitOfWork.Users.AddToRolesAsync(user, command.Request.Roles);

        await cacheService.RemoveByTagAsync(Cache.Tags.Users(currentTenantGetter.GetTenantId()!), cancellationToken);

        var response = (user, command.Request.Roles).Adapt<UserResponse>();

        return Result.Success(response);
    }
}
