namespace PharmaSaaS.Application.Features.Roles.Update;

public sealed class UpdateRoleCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<UpdateRoleCommand, Result>
{
    public async Task<Result> Handle(UpdateRoleCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Roles.GetAsync(command.Id, cancellationToken) is not { } role)
            return Result.Failure(RoleErrors.NotFound);

        if (await unitOfWork.Roles.NameExistsAsync(command.Request.Name, role.Id, cancellationToken))
            return Result.Failure(RoleErrors.DuplicatedName);

        var allowedPermissions = Permissions.GetAll().ToHashSet();

        if (command.Request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure(RoleErrors.InvalidPermissions);

        role.Update(command.Request.Name);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var result = await unitOfWork.Roles.UpdateAsync(role);

            if (result.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync();
                return result;
            }

            var currentPermissions = await unitOfWork.Roles.GetClaimsAsync(role.Id, cancellationToken);
            var permissionsToAdd = command.Request.Permissions.Except(currentPermissions).ToList();
            var permissionsToRemove = currentPermissions.Except(command.Request.Permissions).ToList();

            if (permissionsToRemove.Count != 0)
                await unitOfWork.Roles.DeleteClaimsAsync(role.Id, permissionsToRemove, cancellationToken);

            if (permissionsToAdd.Count != 0)
                await unitOfWork.Roles.AddClaimsAsync(role.Id, Permissions.Type, permissionsToAdd, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await unitOfWork.RollbackTransactionAsync();
            return Result.Failure(RoleErrors.CreationCanceled);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }

        await cacheService.RemoveByTagAsync(Cache.Tags.Roles(currentTenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success();
    }
}
