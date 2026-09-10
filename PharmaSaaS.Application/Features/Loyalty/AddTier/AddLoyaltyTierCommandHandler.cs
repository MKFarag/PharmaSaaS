namespace PharmaSaaS.Application.Features.Loyalty.AddTier;

public sealed class AddLoyaltyTierCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<AddLoyaltyTierCommand, Result<LoyaltyTierResponse>>
{
    public async Task<Result<LoyaltyTierResponse>> Handle(AddLoyaltyTierCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.LoyaltyTiers.AnyAsync(t => t.Name == command.Request.Name, cancellationToken))
            return Result.Failure<LoyaltyTierResponse>(LoyaltyErrors.DuplicatedTierName);

        var tier = LoyaltyTier.Create(command.Request.Name, command.Request.SpendThreshold);

        if (command.Request.DiscountRules?.Any() == true)
        {
            if (!tier.TryAddRules(command.Request.DiscountRules.Select(r => (r.MinInvoiceAmount, r.DiscountPercent))))
                return Result.Failure<LoyaltyTierResponse>(LoyaltyErrors.DuplicatedRuleAmount);
        }
        await unitOfWork.LoyaltyTiers.AddAsync(tier, cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.LoyaltyTiers(tenantGetter.GetTenantId()!), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(tier.Adapt<LoyaltyTierResponse>());
    }
}
