namespace PharmaSaaS.Application.Features.Roles.Add;

public class AddRoleCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<AddRoleCommand, Result<RoleDetailResponse>>
{
    public async Task<Result<RoleDetailResponse>> Handle(AddRoleCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Roles.NameExistsAsync(command.Request.Name, cancellationToken))
            return Result.Failure<RoleDetailResponse>(RoleErrors.DuplicatedName);

        var allowedPermissions = Permissions.GetAll().ToHashSet();

        if (command.Request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure<RoleDetailResponse>(RoleErrors.InvalidPermissions);

        var role = Role.Create(command.Request.Name);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var result = await unitOfWork.Roles.CreateAsync(role);

            if (result.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync();
                return Result.Failure<RoleDetailResponse>(result.Error);
            }

            await unitOfWork.Roles.AddClaimsAsync(role.Id, Permissions.Type, command.Request.Permissions, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result.Failure<RoleDetailResponse>(RoleErrors.CreationCanceled);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }

        await cacheService.RemoveByTagAsync(Cache.Tags.Roles(currentTenantGetter.GetTenantId()!), cancellationToken);

        var response = new RoleDetailResponse(role.Id, role.Name, role.IsDisabled, command.Request.Permissions);

        return Result.Success(response);
    }
}
