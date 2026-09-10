namespace PharmaSaaS.Application.Features.Loyalty.UpdateTier;

public sealed class UpdateLoyaltyTierCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<UpdateLoyaltyTierCommand, Result>
{
    public async Task<Result> Handle(UpdateLoyaltyTierCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.LoyaltyTiers.GetAsync([command.Id], cancellationToken) is not { } tier)
            return Result.Failure(LoyaltyErrors.TierNotFound);

        if (tier.Name != command.Request.Name &&
            await unitOfWork.LoyaltyTiers.AnyAsync(t => t.Name == command.Request.Name, cancellationToken))
            return Result.Failure(LoyaltyErrors.DuplicatedTierName);

        tier.Update(command.Request.Name, command.Request.SpendThreshold);

        unitOfWork.LoyaltyTiers.Update(tier);

        await cacheService.RemoveByTagAsync(Cache.Tags.LoyaltyTiers(tenantGetter.GetTenantId()!), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
