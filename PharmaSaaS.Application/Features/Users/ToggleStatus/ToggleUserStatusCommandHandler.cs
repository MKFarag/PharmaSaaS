namespace PharmaSaaS.Application.Features.Users.ToggleStatus;

public sealed class ToggleUserStatusCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<ToggleUserStatusCommand, Result>
{
    public async Task<Result> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.FindByIdAsync(request.Id, cancellationToken) is not { } user)
            return Result.Failure(UserErrors.NotFound);

        user.ToggleStatus();

        var result = await unitOfWork.Users.UpdateAsync(user);

        await cacheService.RemoveByTagAsync(Cache.Tags.Users(currentTenantGetter.GetTenantId()!), cancellationToken);

        return result;
    }
}
