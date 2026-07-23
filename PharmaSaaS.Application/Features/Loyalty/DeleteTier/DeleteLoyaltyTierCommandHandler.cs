namespace PharmaSaaS.Application.Features.Loyalty.DeleteTier;

public sealed class DeleteLoyaltyTierCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<DeleteLoyaltyTierCommand, Result>
{
    public async Task<Result> Handle(DeleteLoyaltyTierCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.LoyaltyTiers.GetAsync([command.Id], cancellationToken: cancellationToken) is not { } tier)
            return Result.Failure(LoyaltyErrors.TierNotFound);

        if (await unitOfWork.Customers.AnyAsync(c => c.CurrentTierId == command.Id, cancellationToken))
            return Result.Failure(LoyaltyErrors.TierInUse);

        unitOfWork.LoyaltyTiers.Delete(tier);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.LoyaltyTiers(tenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success();
    }
}
