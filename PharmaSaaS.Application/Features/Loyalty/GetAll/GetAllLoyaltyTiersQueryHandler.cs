namespace PharmaSaaS.Application.Features.Loyalty.GetAll;

public sealed class GetAllLoyaltyTiersQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<GetAllLoyaltyTiersQuery, IEnumerable<LoyaltyTierResponse>>
{
    public async Task<IEnumerable<LoyaltyTierResponse>> Handle(GetAllLoyaltyTiersQuery request, CancellationToken cancellationToken = default)
    {
        return await cacheService.GetOrCreateAsync(
            Cache.Keys.AllLoyaltyTiers(tenantGetter.GetTenantId()!),
            async (ct) =>
            {
                var tiers = await unitOfWork.LoyaltyTiers.GetAllAsync([nameof(LoyaltyTier.DiscountRules)], ct);
                return tiers.Adapt<IEnumerable<LoyaltyTierResponse>>();
            },
            Cache.Expirations.Medium,
            [Cache.Tags.LoyaltyTiers(tenantGetter.GetTenantId()!)],
            cancellationToken);
    }
}
