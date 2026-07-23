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
            if (command.Request.DiscountRules.GroupBy(r => r.MinInvoiceAmount).Any(g => g.Count() > 1))
                return Result.Failure<LoyaltyTierResponse>(LoyaltyErrors.DuplicatedRuleAmount);

            foreach (var rule in command.Request.DiscountRules)
                tier.DiscountRules.Add(DiscountRule.Create(0, rule.MinInvoiceAmount, rule.DiscountPercent));
        }
        await unitOfWork.LoyaltyTiers.AddAsync(tier, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.LoyaltyTiers(tenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success(tier.Adapt<LoyaltyTierResponse>());
    }
}
