namespace PharmaSaaS.Application.Features.Roles.ToggleStatus;

public class ToggleRoleStatusCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<ToggleRoleStatusCommand, Result>
{
    public async Task<Result> Handle(ToggleRoleStatusCommand request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Roles.GetAsync(request.Id, cancellationToken) is not { } role)
            return Result.Failure(RoleErrors.NotFound);

        role.ToggleStatus();

        var result = await unitOfWork.Roles.UpdateAsync(role);

        if (result.IsSuccess)
            await cacheService.RemoveByTagAsync(Cache.Tags.Roles(currentTenantGetter.GetTenantId()!), cancellationToken);

        return result;
    }
}
