namespace PharmaSaaS.Application.Features.Loyalty.GetAll;

public sealed class GetAllLoyaltyTiersQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<GetAllLoyaltyTiersQuery, IEnumerable<LoyaltyTierResponse>>
{
    public async Task<IEnumerable<LoyaltyTierResponse>> Handle(GetAllLoyaltyTiersQuery request, CancellationToken cancellationToken = default)
    {
        return await cacheService
            .GetOrCreateAsync
            (
                Cache.Keys.AllLoyaltyTiers(tenantGetter.GetTenantId()!),
                async (ct) => await unitOfWork.LoyaltyTiers
                .GetAllProjectionAsync<LoyaltyTierResponse>([nameof(LoyaltyTier.DiscountRules)], ct),
                Cache.Expirations.VeryLong,
                [Cache.Tags.LoyaltyTiers(tenantGetter.GetTenantId()!)],
                cancellationToken
            );
    }
}
